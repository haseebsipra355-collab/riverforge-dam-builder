using System;
using UnityEngine;

namespace Riverforge
{
    public sealed class DamSimulation : MonoBehaviour
    {
        public DamGameState State { get; private set; }
        public bool IsPaused { get; private set; }
        public float TimeScale { get; private set; } = 1f;
        public float CurrentPowerMW { get; private set; }
        public float CurrentInflow { get; private set; }
        public float CurrentOutflow { get; private set; }
        public string MissionText => "Build the dam, generate 1,000 MWh, survive through Day 5, and keep safety above 70%.";

        public event Action<DamGameState> Changed;
        public event Action<string> Message;
        public event Action<string> WeatherChanged;

        private System.Random _rng;
        private float _weatherTimer;
        private float _autosaveTimer;
        private const float SecondsPerGameHour = 4f;
        private const float BuildSeconds = 18f;

        public void Initialize(bool loadSave)
        {
            if (loadSave && SaveSystem.TryLoad(out var loaded)) State = loaded;
            else State = new DamGameState();

            _rng = new System.Random(State.seed + State.day);
            _weatherTimer = 25f;
            Emit("Project ready. Survey the valley and start construction.");
            RaiseChanged();
        }

        private void Update()
        {
            if (State == null || IsPaused || State.won || State.failed) return;
            var dt = Time.unscaledDeltaTime * TimeScale;
            TickClock(dt);
            TickWeather(dt);
            TickConstruction(dt);
            TickHydrology(dt);
            TickEconomy(dt);
            TickSafetyAndEcology(dt);
            TickGoals();

            _autosaveTimer += dt;
            if (_autosaveTimer >= 20f)
            {
                _autosaveTimer = 0f;
                SaveSystem.Save(State);
            }
            RaiseChanged();
        }

        private void TickClock(float dt)
        {
            State.hour += dt / SecondsPerGameHour;
            if (State.hour < 24f) return;
            State.hour -= 24f;
            State.day++;
            State.money += Mathf.Max(0f, State.publicTrust - 60f) * 45f;
            Emit($"Day {State.day} begins. Regional demand and rainfall forecasts have updated.");
        }

        private void TickWeather(float dt)
        {
            _weatherTimer -= dt;
            if (_weatherTimer > 0f) return;
            _weatherTimer = Mathf.Lerp(28f, 48f, (float)_rng.NextDouble());
            var roll = _rng.NextDouble();
            string next = roll < .48 ? "Clear" : roll < .70 ? "Rain" : roll < .86 ? "Storm" : "Drought";
            if (next == State.weather) next = "Clear";
            State.weather = next;
            WeatherChanged?.Invoke(next);
            Emit(next == "Storm" ? "Severe storm warning: inflow is rising fast." : $"Weather changed: {next}.");
        }

        private void TickConstruction(float dt)
        {
            if (State.damBuilt || State.buildProgress <= 0f) return;
            State.buildProgress = Mathf.Min(1f, State.buildProgress + dt / BuildSeconds);
            if (State.buildProgress < 1f) return;
            State.damBuilt = true;
            State.turbineLoad = .35f;
            Emit("Dam commissioned. Turbine 1 is synchronized to the grid.");
        }

        private void TickHydrology(float dt)
        {
            float weatherInflow = State.weather switch
            {
                "Rain" => 0.0062f,
                "Storm" => 0.012f,
                "Drought" => 0.0012f,
                _ => 0.0032f
            };

            CurrentInflow = weatherInflow * 1000f;
            float turbineRelease = State.damBuilt ? State.turbineLoad * (0.0036f + State.turbineLevel * 0.0006f) : 0f;
            float gateRelease = State.damBuilt ? State.spillway * (0.006f + State.spillwayLevel * 0.0014f) : 0.0022f;
            CurrentOutflow = (turbineRelease + gateRelease) * 1000f;

            float net = weatherInflow - turbineRelease - gateRelease;
            State.waterLevel = Mathf.Clamp(State.waterLevel + net * dt, .05f, 1.18f);

            float head = Mathf.InverseLerp(.18f, 1f + State.wallLevel * .08f, State.waterLevel);
            float capacity = 38f + State.turbineLevel * 16f;
            CurrentPowerMW = State.damBuilt ? capacity * State.turbineLoad * Mathf.Lerp(.28f, 1f, head) : 0f;
            State.totalEnergyMWh += CurrentPowerMW * (dt / 3600f) * 90f;
        }

        private void TickEconomy(float dt)
        {
            if (!State.damBuilt) return;
            const float revenuePerMWh = 92f;
            float simulatedHours = dt / SecondsPerGameHour;
            State.money += CurrentPowerMW * simulatedHours * revenuePerMWh;
            State.money -= (12f + CurrentPowerMW * .12f) * dt;
        }

        private void TickSafetyAndEcology(float dt)
        {
            float safeCapacity = 0.94f + State.wallLevel * .06f;
            if (State.waterLevel > safeCapacity)
            {
                float pressure = (State.waterLevel - safeCapacity) / .2f;
                State.safety -= pressure * (State.weather == "Storm" ? 2.4f : 1.4f) * dt;
                State.publicTrust -= pressure * .24f * dt;
            }
            else if (State.damBuilt)
            {
                State.safety = Mathf.Min(100f, State.safety + .035f * dt);
            }

            float environmentalFlow = State.damBuilt ? State.turbineLoad * .55f + State.spillway * .75f : .7f;
            if (environmentalFlow < .22f) State.ecology -= (.22f - environmentalFlow) * .48f * dt;
            else State.ecology = Mathf.Min(100f, State.ecology + .014f * dt);

            if (State.spillway > .85f) State.ecology -= .02f * dt;
            State.safety = Mathf.Clamp(State.safety, 0f, 100f);
            State.ecology = Mathf.Clamp(State.ecology, 0f, 100f);
            State.publicTrust = Mathf.Clamp(State.publicTrust, 0f, 100f);
        }

        private void TickGoals()
        {
            if (State.safety <= 0f)
            {
                State.failed = true;
                Emit("Dam failure. The project has been shut down.");
                SaveSystem.Save(State);
                return;
            }
            if (State.ecology <= 0f || State.publicTrust <= 0f)
            {
                State.failed = true;
                Emit("Project licence revoked after environmental and public-trust collapse.");
                SaveSystem.Save(State);
                return;
            }
            if (!State.won && State.day >= 5 && State.totalEnergyMWh >= 1000f && State.safety >= 70f)
            {
                State.won = true;
                State.money += 50000f;
                Emit("Mission complete. Riverforge is now a stable regional power asset.");
                SaveSystem.Save(State);
            }
        }

        public bool StartDamConstruction()
        {
            if (State.damBuilt || State.buildProgress > 0f || State.money < 32000f) return false;
            State.money -= 32000f;
            State.buildProgress = .001f;
            Emit("Construction started: access road, excavation, and foundation works underway.");
            RaiseChanged();
            return true;
        }

        public bool BuyWallUpgrade()
        {
            float cost = 18000f + State.wallLevel * 16000f;
            if (!State.damBuilt || State.wallLevel >= 3 || State.money < cost) return false;
            State.money -= cost; State.wallLevel++; State.safety = Mathf.Min(100f, State.safety + 8f);
            Emit($"Dam wall upgraded to Level {State.wallLevel}. Safe reservoir capacity increased."); RaiseChanged(); return true;
        }

        public bool BuyTurbineUpgrade()
        {
            float cost = 14000f + State.turbineLevel * 12000f;
            if (!State.damBuilt || State.turbineLevel >= 3 || State.money < cost) return false;
            State.money -= cost; State.turbineLevel++;
            Emit($"Turbine hall upgraded to Level {State.turbineLevel}. Generation capacity increased."); RaiseChanged(); return true;
        }

        public bool BuySpillwayUpgrade()
        {
            float cost = 12000f + State.spillwayLevel * 10000f;
            if (!State.damBuilt || State.spillwayLevel >= 3 || State.money < cost) return false;
            State.money -= cost; State.spillwayLevel++;
            Emit($"Spillway upgraded to Level {State.spillwayLevel}. Emergency discharge capacity increased."); RaiseChanged(); return true;
        }

        public void SetTurbine(float value) { State.turbineLoad = Mathf.Clamp01(value); RaiseChanged(); }
        public void SetSpillway(float value) { State.spillway = Mathf.Clamp01(value); RaiseChanged(); }
        public void EmergencyRelease() { if (!State.damBuilt) return; State.spillway = 1f; Emit("Emergency spillway release activated."); RaiseChanged(); }
        public void TogglePause() { IsPaused = !IsPaused; RaiseChanged(); }
        public void CycleSpeed() { TimeScale = TimeScale < 1.5f ? 2f : TimeScale < 3f ? 4f : 1f; RaiseChanged(); }
        public void NewGame() { SaveSystem.Clear(); State = new DamGameState(); _rng = new System.Random(State.seed); IsPaused = false; TimeScale = 1f; Emit("New project started."); RaiseChanged(); }
        public void SaveNow() { SaveSystem.Save(State); Emit("Game saved."); }

        private void Emit(string text) => Message?.Invoke(text);
        private void RaiseChanged() => Changed?.Invoke(State);
    }
}
