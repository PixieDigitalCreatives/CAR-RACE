using UnityEngine;
using UnityEngine.Rendering;

namespace ThegameioRacing.Vehicle
{
    /// <summary>
    /// Constructs 3D procedural vehicle bodies, wheels, materials, colliders,
    /// dynamic night headlights/taillights, and attaches WheelColliders and VehicleController.
    /// Provides 5 distinct silhouettes matching the vehicle roles from thegameio_Racing_Game.ppt.
    /// </summary>
    public static class CarModelBuilder
    {
        public static GameObject BuildProceduralCar(string name, VehicleConfig config)
        {
            GameObject carRoot = new GameObject(name);
            carRoot.tag = "Player";

            // Rigidbody setup (Unity 6 damping properties)
            Rigidbody rb = carRoot.AddComponent<Rigidbody>();
            rb.mass = config != null ? config.mass : 1350f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 2.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Vehicle Controller
            VehicleController controller = carRoot.AddComponent<VehicleController>();
            if (config != null) controller.SetConfig(config);

            // Shaders & Materials (Universal Render Pipeline Lit)
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Color bodyCol = config != null ? config.primaryColor : new Color(0.12f, 0.45f, 0.95f);
            Color accentCol = config != null ? config.accentColor : new Color(0.98f, 0.55f, 0.12f);

            Material matBody = CreateMaterial(litShader, "Mat_CarBody_" + name, bodyCol, 0.85f, 0.75f);
            Material matAccent = CreateMaterial(litShader, "Mat_CarAccent_" + name, accentCol, 0.8f, 0.8f);
            Material matGlass = CreateMaterial(litShader, "Mat_CarGlass", new Color(0.06f, 0.08f, 0.12f), 0.95f, 0.2f);
            Material matTrim = CreateMaterial(litShader, "Mat_CarTrim", new Color(0.12f, 0.12f, 0.14f), 0.35f, 0.15f);
            Material matChrome = CreateMaterial(litShader, "Mat_CarChrome", new Color(0.92f, 0.92f, 0.95f), 0.95f, 0.95f);
            Material matTire = CreateMaterial(litShader, "Mat_TireRubber", new Color(0.12f, 0.12f, 0.13f), 0.1f, 0.0f);
            Material matRim = CreateMaterial(litShader, "Mat_AlloyRim", new Color(0.80f, 0.82f, 0.86f), 0.85f, 0.85f);
            Material matGoldRim = CreateMaterial(litShader, "Mat_GoldRim", new Color(0.85f, 0.70f, 0.25f), 0.90f, 0.85f);

            Color headlightEmission = (config != null && config.role == VehicleRole.TunerDrift)
                ? new Color(2.5f, 2.2f, 0.5f) // JDM style golden-amber headlights
                : new Color(1.8f, 2.2f, 2.8f); // Xenon cool white

            Material matHeadlights = CreateEmissiveMaterial(litShader, "Mat_Headlights", new Color(0.9f, 0.95f, 1f), headlightEmission);
            Material matTaillights = CreateEmissiveMaterial(litShader, "Mat_Taillights", new Color(0.85f, 0.1f, 0.1f), new Color(3.0f, 0.15f, 0.15f));
            Material matAmberFog = CreateEmissiveMaterial(litShader, "Mat_FogLights", new Color(1.0f, 0.7f, 0.1f), new Color(2.8f, 1.8f, 0.2f));

            // Chassis Visual Root
            GameObject visualRoot = new GameObject("VisualChassis");
            visualRoot.transform.SetParent(carRoot.transform, false);

            VehicleRole role = config != null ? config.role : VehicleRole.StarterBalanced;
            float wheelRadius = 0.36f;
            float wheelWidth = 0.28f;
            float trackHalfWidth = 0.86f;
            float frontZ = 1.35f;
            float rearZ = -1.35f;
            float wheelY = 0.36f;

            // Build role-specific 3D body silhouette
            switch (role)
            {
                case VehicleRole.HypercarSpeed:
                    wheelRadius = 0.35f;
                    wheelWidth = 0.32f;
                    trackHalfWidth = 0.92f;
                    frontZ = 1.45f;
                    rearZ = -1.40f;
                    BuildHypercarSilhouette(visualRoot.transform, matBody, matAccent, matGlass, matTrim, matHeadlights, matTaillights);
                    break;

                case VehicleRole.ClassicMuscle:
                    wheelRadius = 0.37f;
                    wheelWidth = 0.30f;
                    trackHalfWidth = 0.88f;
                    frontZ = 1.40f;
                    rearZ = -1.30f;
                    BuildClassicMuscleSilhouette(visualRoot.transform, matBody, matAccent, matGlass, matTrim, matChrome, matHeadlights, matTaillights);
                    break;

                case VehicleRole.TunerDrift:
                    wheelRadius = 0.35f;
                    wheelWidth = 0.30f;
                    trackHalfWidth = 0.90f;
                    frontZ = 1.32f;
                    rearZ = -1.32f;
                    BuildTunerDriftSilhouette(visualRoot.transform, matBody, matAccent, matGlass, matTrim, matHeadlights, matTaillights);
                    break;

                case VehicleRole.StreetRally:
                    wheelRadius = 0.40f;
                    wheelWidth = 0.30f;
                    trackHalfWidth = 0.88f;
                    frontZ = 1.30f;
                    rearZ = -1.30f;
                    wheelY = 0.42f; // Raised safari suspension
                    BuildStreetRallySilhouette(visualRoot.transform, matBody, matAccent, matGlass, matTrim, matHeadlights, matTaillights, matAmberFog);
                    break;

                case VehicleRole.StarterBalanced:
                default:
                    wheelRadius = 0.36f;
                    wheelWidth = 0.28f;
                    trackHalfWidth = 0.86f;
                    frontZ = 1.35f;
                    rearZ = -1.35f;
                    BuildStarterBalancedSilhouette(visualRoot.transform, matBody, matAccent, matGlass, matTrim, matHeadlights, matTaillights);
                    break;
            }

            // Real-Time Night Illumination (Spotlights for headlights, pointlights for taillights)
            CreateVehicleLighting(visualRoot.transform, role, headlightEmission);

            // Body Collider (Composite BoxCollider on carRoot)
            BoxCollider bodyCollider = carRoot.AddComponent<BoxCollider>();
            bodyCollider.center = new Vector3(0f, 0.55f, 0f);
            bodyCollider.size = new Vector3(trackHalfWidth * 2.1f, 0.72f, 4.35f);

            // Wheel Rims: Gold for TunerDrift, Chrome for Muscle, Alloy for others
            Material selectedRimMat = role == VehicleRole.TunerDrift ? matGoldRim : (role == VehicleRole.ClassicMuscle ? matChrome : matRim);

            // Setup Wheels
            CreateWheel(carRoot, "Wheel_FL", new Vector3(-trackHalfWidth, wheelY, frontZ), wheelRadius, wheelWidth, matTire, selectedRimMat,
                out WheelCollider flC, out Transform flV);
            CreateWheel(carRoot, "Wheel_FR", new Vector3(trackHalfWidth, wheelY, frontZ), wheelRadius, wheelWidth, matTire, selectedRimMat,
                out WheelCollider frC, out Transform frV);
            CreateWheel(carRoot, "Wheel_RL", new Vector3(-trackHalfWidth, wheelY, rearZ), wheelRadius, wheelWidth, matTire, selectedRimMat,
                out WheelCollider rlC, out Transform rlV);
            CreateWheel(carRoot, "Wheel_RR", new Vector3(trackHalfWidth, wheelY, rearZ), wheelRadius, wheelWidth, matTire, selectedRimMat,
                out WheelCollider rrC, out Transform rrV);

            // Skid trails
            TrailRenderer[] trails = new TrailRenderer[2];
            trails[0] = CreateSkidTrail("SkidTrail_RL", rlV, wheelRadius);
            trails[1] = CreateSkidTrail("SkidTrail_RR", rrV, wheelRadius);

            controller.SetupComponents(flC, frC, rlC, rrC, flV, frV, rlV, rrV, trails);

            // Dynamic 3D procedural combustion engine audio
            ThegameioRacing.Audio.ProceduralAudio.AttachEngineAudio(controller);

            return carRoot;
        }

        #region Vehicle Silhouettes

        /// <summary>
        /// Hypercar: Ultra-low wedge body, extreme rear GT wing, aggressive front splitter, diffuser channels.
        /// </summary>
        private static void BuildHypercarSilhouette(Transform parent, Material matBody, Material matAccent, Material matGlass, Material matTrim, Material matHeadlights, Material matTaillights)
        {
            // Lower Tub
            CreateBoxPart("LowerTub", parent, new Vector3(0f, 0.34f, 0f), new Vector3(1.95f, 0.28f, 4.5f), matBody);
            // Low Slung Cockpit / Teardrop Canopy
            CreateBoxPart("Canopy", parent, new Vector3(0f, 0.74f, -0.1f), new Vector3(1.30f, 0.44f, 2.2f), matGlass);
            // Wedge Hood Slope
            CreateBoxPart("WedgeNose", parent, new Vector3(0f, 0.46f, 1.4f), new Vector3(1.80f, 0.20f, 1.6f), matBody);
            // Front Aero Splitter & Canards
            CreateBoxPart("CarbonSplitter", parent, new Vector3(0f, 0.16f, 2.32f), new Vector3(2.02f, 0.08f, 0.45f), matTrim);
            CreateBoxPart("Canard_L", parent, new Vector3(-0.98f, 0.32f, 2.15f), new Vector3(0.12f, 0.18f, 0.35f), matAccent);
            CreateBoxPart("Canard_R", parent, new Vector3(0.98f, 0.32f, 2.15f), new Vector3(0.12f, 0.18f, 0.35f), matAccent);
            // Side Aero Inlets
            CreateBoxPart("SidePod_L", parent, new Vector3(-0.96f, 0.38f, -0.4f), new Vector3(0.18f, 0.28f, 1.8f), matAccent);
            CreateBoxPart("SidePod_R", parent, new Vector3(0.96f, 0.38f, -0.4f), new Vector3(0.18f, 0.28f, 1.8f), matAccent);
            // Rear Long Deck
            CreateBoxPart("RearDeck", parent, new Vector3(0f, 0.52f, -1.55f), new Vector3(1.82f, 0.22f, 1.3f), matBody);
            // Extreme Carbon GT Wing
            GameObject wing = new GameObject("GTWing");
            wing.transform.SetParent(parent, false);
            CreateBoxPart("WingBlade", wing.transform, new Vector3(0f, 1.05f, -2.15f), new Vector3(1.92f, 0.05f, 0.42f), matAccent);
            CreateBoxPart("WingEndplate_L", wing.transform, new Vector3(-0.95f, 1.05f, -2.15f), new Vector3(0.04f, 0.26f, 0.44f), matTrim);
            CreateBoxPart("WingEndplate_R", wing.transform, new Vector3(0.95f, 1.05f, -2.15f), new Vector3(0.04f, 0.26f, 0.44f), matTrim);
            CreateBoxPart("Pillar_L", wing.transform, new Vector3(-0.45f, 0.80f, -2.05f), new Vector3(0.06f, 0.45f, 0.18f), matTrim);
            CreateBoxPart("Pillar_R", wing.transform, new Vector3(0.45f, 0.80f, -2.05f), new Vector3(0.06f, 0.45f, 0.18f), matTrim);
            // Rear Diffuser
            CreateBoxPart("Diffuser", parent, new Vector3(0f, 0.20f, -2.25f), new Vector3(1.90f, 0.16f, 0.35f), matTrim);
            // Slim LED Headlights & Taillight Blade
            CreateBoxPart("Headlight_L", parent, new Vector3(-0.70f, 0.46f, 2.22f), new Vector3(0.42f, 0.06f, 0.10f), matHeadlights);
            CreateBoxPart("Headlight_R", parent, new Vector3(0.70f, 0.46f, 2.22f), new Vector3(0.42f, 0.06f, 0.10f), matHeadlights);
            CreateBoxPart("TaillightBlade", parent, new Vector3(0f, 0.54f, -2.26f), new Vector3(1.82f, 0.06f, 0.08f), matTaillights);
        }

        /// <summary>
        /// Classic Muscle: Bold blocky shoulders, protruding shaker hood scoop, fastback roof, chrome bumpers.
        /// </summary>
        private static void BuildClassicMuscleSilhouette(Transform parent, Material matBody, Material matAccent, Material matGlass, Material matTrim, Material matChrome, Material matHeadlights, Material matTaillights)
        {
            // Muscular Lower Tub
            CreateBoxPart("LowerBody", parent, new Vector3(0f, 0.42f, 0f), new Vector3(1.90f, 0.42f, 4.4f), matBody);
            // Fastback Cabin
            CreateBoxPart("CabinRoof", parent, new Vector3(0f, 0.95f, -0.25f), new Vector3(1.44f, 0.54f, 2.2f), matGlass);
            // Long Bulging Hood
            CreateBoxPart("Hood", parent, new Vector3(0f, 0.65f, 1.25f), new Vector3(1.82f, 0.26f, 1.7f), matBody);
            // Exposed Shaker Hood Scoop
            CreateBoxPart("HoodScoop", parent, new Vector3(0f, 0.82f, 1.15f), new Vector3(0.60f, 0.14f, 0.65f), matTrim);
            // Front Heavy Chrome Bumper & Grille
            CreateBoxPart("FrontChromeBumper", parent, new Vector3(0f, 0.32f, 2.22f), new Vector3(1.94f, 0.18f, 0.22f), matChrome);
            CreateBoxPart("FrontGrille", parent, new Vector3(0f, 0.52f, 2.18f), new Vector3(1.68f, 0.20f, 0.12f), matTrim);
            // Rear Chrome Bumper & Trunk Lip
            CreateBoxPart("RearChromeBumper", parent, new Vector3(0f, 0.34f, -2.22f), new Vector3(1.94f, 0.18f, 0.22f), matChrome);
            CreateBoxPart("TrunkLip", parent, new Vector3(0f, 0.72f, -2.12f), new Vector3(1.78f, 0.10f, 0.18f), matAccent);
            // Dual Round Retro Headlights
            CreateBoxPart("Headlight_L1", parent, new Vector3(-0.68f, 0.52f, 2.21f), new Vector3(0.18f, 0.18f, 0.08f), matHeadlights);
            CreateBoxPart("Headlight_L2", parent, new Vector3(-0.46f, 0.52f, 2.21f), new Vector3(0.18f, 0.18f, 0.08f), matHeadlights);
            CreateBoxPart("Headlight_R1", parent, new Vector3(0.46f, 0.52f, 2.21f), new Vector3(0.18f, 0.18f, 0.08f), matHeadlights);
            CreateBoxPart("Headlight_R2", parent, new Vector3(0.68f, 0.52f, 2.21f), new Vector3(0.18f, 0.18f, 0.08f), matHeadlights);
            // Wide Triple-Segment Taillights
            CreateBoxPart("Taillight_L", parent, new Vector3(-0.62f, 0.56f, -2.22f), new Vector3(0.55f, 0.16f, 0.08f), matTaillights);
            CreateBoxPart("Taillight_R", parent, new Vector3(0.62f, 0.56f, -2.22f), new Vector3(0.55f, 0.16f, 0.08f), matTaillights);
        }

        /// <summary>
        /// Tuner Drift: Widebody overfenders, exposed intercooler, aggressive drift spoiler, low ground hugging kit.
        /// </summary>
        private static void BuildTunerDriftSilhouette(Transform parent, Material matBody, Material matAccent, Material matGlass, Material matTrim, Material matHeadlights, Material matTaillights)
        {
            // Sleek JDM Tub
            CreateBoxPart("LowerBody", parent, new Vector3(0f, 0.38f, 0f), new Vector3(1.82f, 0.34f, 4.3f), matBody);
            // Low Roof Coupe
            CreateBoxPart("CoupeCabin", parent, new Vector3(0f, 0.86f, -0.15f), new Vector3(1.36f, 0.50f, 2.1f), matGlass);
            // Sloping Front Hood
            CreateBoxPart("VentedHood", parent, new Vector3(0f, 0.54f, 1.25f), new Vector3(1.70f, 0.22f, 1.45f), matBody);
            CreateBoxPart("HoodVents", parent, new Vector3(0f, 0.66f, 1.15f), new Vector3(0.80f, 0.04f, 0.55f), matTrim);
            // Bolt-On Widebody Overfenders (Front & Rear)
            CreateBoxPart("Fender_FL", parent, new Vector3(-0.95f, 0.40f, 1.32f), new Vector3(0.16f, 0.28f, 0.85f), matAccent);
            CreateBoxPart("Fender_FR", parent, new Vector3(0.95f, 0.40f, 1.32f), new Vector3(0.16f, 0.28f, 0.85f), matAccent);
            CreateBoxPart("Fender_RL", parent, new Vector3(-0.95f, 0.40f, -1.32f), new Vector3(0.16f, 0.28f, 0.85f), matAccent);
            CreateBoxPart("Fender_RR", parent, new Vector3(0.95f, 0.40f, -1.32f), new Vector3(0.16f, 0.28f, 0.85f), matAccent);
            // Front Aero Bumper with Exposed Aluminum Intercooler
            CreateBoxPart("DriftBumper", parent, new Vector3(0f, 0.26f, 2.18f), new Vector3(1.92f, 0.22f, 0.35f), matTrim);
            CreateBoxPart("Intercooler", parent, new Vector3(0f, 0.26f, 2.22f), new Vector3(0.90f, 0.18f, 0.10f), matAccent);
            // High Angle Drift Spoiler
            GameObject spoiler = new GameObject("DriftWing");
            spoiler.transform.SetParent(parent, false);
            CreateBoxPart("WingPlane", spoiler.transform, new Vector3(0f, 1.08f, -1.95f), new Vector3(1.78f, 0.05f, 0.38f), matAccent);
            CreateBoxPart("WingMount_L", spoiler.transform, new Vector3(-0.55f, 0.86f, -1.92f), new Vector3(0.05f, 0.40f, 0.14f), matTrim);
            CreateBoxPart("WingMount_R", spoiler.transform, new Vector3(0.55f, 0.86f, -1.92f), new Vector3(0.05f, 0.40f, 0.14f), matTrim);
            // Tuner Headlights & Taillights
            CreateBoxPart("Headlight_L", parent, new Vector3(-0.66f, 0.48f, 2.16f), new Vector3(0.36f, 0.12f, 0.08f), matHeadlights);
            CreateBoxPart("Headlight_R", parent, new Vector3(0.66f, 0.48f, 2.16f), new Vector3(0.36f, 0.12f, 0.08f), matHeadlights);
            CreateBoxPart("Taillight_L", parent, new Vector3(-0.65f, 0.58f, -2.16f), new Vector3(0.44f, 0.12f, 0.08f), matTaillights);
            CreateBoxPart("Taillight_R", parent, new Vector3(0.65f, 0.58f, -2.16f), new Vector3(0.44f, 0.12f, 0.08f), matTaillights);
        }

        /// <summary>
        /// Street Rally: Raised safari suspension, heavy-duty tubular bull bar, 4 fog light pods, roof scoop, mudflaps.
        /// </summary>
        private static void BuildStreetRallySilhouette(Transform parent, Material matBody, Material matAccent, Material matGlass, Material matTrim, Material matHeadlights, Material matTaillights, Material matAmberFog)
        {
            // Rugged High-Clearance Tub
            CreateBoxPart("LowerBody", parent, new Vector3(0f, 0.48f, 0f), new Vector3(1.86f, 0.38f, 4.3f), matBody);
            // Upright Rally Hatch/Coupe Cabin
            CreateBoxPart("RallyCabin", parent, new Vector3(0f, 0.98f, -0.15f), new Vector3(1.42f, 0.56f, 2.2f), matGlass);
            // Sloped Reinforced Hood
            CreateBoxPart("RallyHood", parent, new Vector3(0f, 0.66f, 1.25f), new Vector3(1.72f, 0.26f, 1.4f), matBody);
            // Roof Air Intake Scoop
            CreateBoxPart("RoofScoop", parent, new Vector3(0f, 1.30f, 0.35f), new Vector3(0.42f, 0.14f, 0.55f), matAccent);
            // Heavy Bull Bar (Front Skid Protection)
            CreateBoxPart("BullBar_Lower", parent, new Vector3(0f, 0.30f, 2.25f), new Vector3(1.65f, 0.12f, 0.18f), matTrim);
            CreateBoxPart("BullBar_Upper", parent, new Vector3(0f, 0.54f, 2.28f), new Vector3(1.10f, 0.10f, 0.12f), matTrim);
            CreateBoxPart("BullBar_PostL", parent, new Vector3(-0.45f, 0.42f, 2.26f), new Vector3(0.08f, 0.26f, 0.12f), matTrim);
            CreateBoxPart("BullBar_PostR", parent, new Vector3(0.45f, 0.42f, 2.26f), new Vector3(0.08f, 0.26f, 0.12f), matTrim);
            // 4 Auxiliary Round Fog Light Pods on Bull Bar
            CreateBoxPart("RallyFog_1", parent, new Vector3(-0.42f, 0.60f, 2.32f), new Vector3(0.16f, 0.16f, 0.10f), matAmberFog);
            CreateBoxPart("RallyFog_2", parent, new Vector3(-0.14f, 0.60f, 2.32f), new Vector3(0.16f, 0.16f, 0.10f), matAmberFog);
            CreateBoxPart("RallyFog_3", parent, new Vector3(0.14f, 0.60f, 2.32f), new Vector3(0.16f, 0.16f, 0.10f), matAmberFog);
            CreateBoxPart("RallyFog_4", parent, new Vector3(0.42f, 0.60f, 2.32f), new Vector3(0.16f, 0.16f, 0.10f), matAmberFog);
            // Rear Hatch Wing
            CreateBoxPart("RallyWing", parent, new Vector3(0f, 1.25f, -1.25f), new Vector3(1.50f, 0.08f, 0.32f), matAccent);
            // 4 Mudflaps
            CreateBoxPart("Mudflap_FL", parent, new Vector3(-0.92f, 0.25f, 0.85f), new Vector3(0.04f, 0.35f, 0.24f), matTrim);
            CreateBoxPart("Mudflap_FR", parent, new Vector3(0.92f, 0.25f, 0.85f), new Vector3(0.04f, 0.35f, 0.24f), matTrim);
            CreateBoxPart("Mudflap_RL", parent, new Vector3(-0.92f, 0.25f, -1.75f), new Vector3(0.04f, 0.35f, 0.24f), matTrim);
            CreateBoxPart("Mudflap_RR", parent, new Vector3(0.92f, 0.25f, -1.75f), new Vector3(0.04f, 0.35f, 0.24f), matTrim);
            // Standard Headlights & Taillights
            CreateBoxPart("Headlight_L", parent, new Vector3(-0.66f, 0.60f, 2.18f), new Vector3(0.35f, 0.12f, 0.08f), matHeadlights);
            CreateBoxPart("Headlight_R", parent, new Vector3(0.66f, 0.60f, 2.18f), new Vector3(0.35f, 0.12f, 0.08f), matHeadlights);
            CreateBoxPart("Taillight_L", parent, new Vector3(-0.65f, 0.72f, -2.16f), new Vector3(0.42f, 0.14f, 0.08f), matTaillights);
            CreateBoxPart("Taillight_R", parent, new Vector3(0.65f, 0.72f, -2.16f), new Vector3(0.42f, 0.14f, 0.08f), matTaillights);
        }

        /// <summary>
        /// Starter Balanced: Cheetah GT modern urban sports coupe.
        /// </summary>
        private static void BuildStarterBalancedSilhouette(Transform parent, Material matBody, Material matAccent, Material matGlass, Material matTrim, Material matHeadlights, Material matTaillights)
        {
            // Lower Body / Tub
            CreateBoxPart("LowerBody", parent, new Vector3(0f, 0.42f, 0f), new Vector3(1.85f, 0.38f, 4.3f), matBody);
            // Cabin / Greenhouse
            CreateBoxPart("CabinRoof", parent, new Vector3(0f, 0.88f, -0.2f), new Vector3(1.42f, 0.52f, 2.1f), matGlass);
            // Front Hood Slope
            CreateBoxPart("FrontHood", parent, new Vector3(0f, 0.56f, 1.25f), new Vector3(1.72f, 0.24f, 1.4f), matBody);
            // Rear Deck & Trunk
            CreateBoxPart("RearDeck", parent, new Vector3(0f, 0.62f, -1.45f), new Vector3(1.72f, 0.26f, 1.1f), matBody);
            // Front Splitter / Bumper
            CreateBoxPart("FrontSplitter", parent, new Vector3(0f, 0.22f, 2.15f), new Vector3(1.88f, 0.14f, 0.35f), matTrim);
            // Rear Diffuser
            CreateBoxPart("RearDiffuser", parent, new Vector3(0f, 0.25f, -2.15f), new Vector3(1.82f, 0.18f, 0.3f), matTrim);
            // Side Skirts
            CreateBoxPart("SideSkirt_L", parent, new Vector3(-0.93f, 0.24f, 0f), new Vector3(0.08f, 0.14f, 2.5f), matTrim);
            CreateBoxPart("SideSkirt_R", parent, new Vector3(0.93f, 0.24f, 0f), new Vector3(0.08f, 0.14f, 2.5f), matTrim);
            // Rear Lip Spoiler
            GameObject spoiler = new GameObject("RearSpoiler");
            spoiler.transform.SetParent(parent, false);
            CreateBoxPart("SpoilerWing", spoiler.transform, new Vector3(0f, 0.98f, -1.92f), new Vector3(1.68f, 0.06f, 0.36f), matAccent);
            CreateBoxPart("SpoilerPillar_L", spoiler.transform, new Vector3(-0.55f, 0.82f, -1.90f), new Vector3(0.05f, 0.28f, 0.15f), matTrim);
            CreateBoxPart("SpoilerPillar_R", spoiler.transform, new Vector3(0.55f, 0.82f, -1.90f), new Vector3(0.05f, 0.28f, 0.15f), matTrim);
            // Headlights & Taillights
            CreateBoxPart("Headlight_L", parent, new Vector3(-0.65f, 0.52f, 2.14f), new Vector3(0.38f, 0.10f, 0.08f), matHeadlights);
            CreateBoxPart("Headlight_R", parent, new Vector3(0.65f, 0.52f, 2.14f), new Vector3(0.38f, 0.10f, 0.08f), matHeadlights);
            CreateBoxPart("Taillight_L", parent, new Vector3(-0.65f, 0.62f, -2.14f), new Vector3(0.48f, 0.10f, 0.08f), matTaillights);
            CreateBoxPart("Taillight_R", parent, new Vector3(0.65f, 0.62f, -2.14f), new Vector3(0.48f, 0.10f, 0.08f), matTaillights);
        }

        #endregion

        #region Lighting & Props

        /// <summary>
        /// Adds dual forward projector spotlights and dual rear red glow lights for authentic night racing.
        /// </summary>
        private static void CreateVehicleLighting(Transform parent, VehicleRole role, Color headlightColor)
        {
            GameObject lightsRoot = new GameObject("VehicleLights");
            lightsRoot.transform.SetParent(parent, false);

            float lightY = (role == VehicleRole.StreetRally) ? 0.60f : 0.50f;
            float lightZ = 2.15f;
            float lightX = 0.65f;

            // Headlight Left Spot
            CreateHeadlightSpot("Spotlight_FL", lightsRoot.transform, new Vector3(-lightX, lightY, lightZ), -1.5f, headlightColor);
            // Headlight Right Spot
            CreateHeadlightSpot("Spotlight_FR", lightsRoot.transform, new Vector3(lightX, lightY, lightZ), 1.5f, headlightColor);

            // Taillight Left Glow
            CreateTaillightPoint("Taillight_RL", lightsRoot.transform, new Vector3(-lightX, lightY + 0.1f, -2.15f));
            // Taillight Right Glow
            CreateTaillightPoint("Taillight_RR", lightsRoot.transform, new Vector3(lightX, lightY + 0.1f, -2.15f));
        }

        private static void CreateHeadlightSpot(string name, Transform parent, Vector3 localPos, float yawOffset, Color color)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(parent, false);
            lightObj.transform.localPosition = localPos;
            // Aim forward, tilted 3 degrees down onto asphalt, and slightly outward
            lightObj.transform.localRotation = Quaternion.Euler(3.5f, yawOffset, 0f);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = 2.8f;
            light.range = 45f;
            light.spotAngle = 55f;
            light.innerSpotAngle = 32f;
            light.shadows = LightShadows.None; // None for mobile performance
        }

        private static void CreateTaillightPoint(string name, Transform parent, Vector3 localPos)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(parent, false);
            lightObj.transform.localPosition = localPos;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.08f, 0.08f);
            light.intensity = 1.4f;
            light.range = 4.5f;
            light.shadows = LightShadows.None;
        }

        #endregion

        #region Helpers

        private static void CreateBoxPart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            // Remove default primitive collider so it does not interfere with chassis BoxCollider / WheelColliders
            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
        }

        private static void CreateWheel(
            GameObject root, string name, Vector3 localPos, float radius, float width,
            Material matTire, Material matRim,
            out WheelCollider collider, out Transform visual)
        {
            // Wheel Collider Object
            GameObject wcObj = new GameObject(name + "_Collider");
            wcObj.transform.SetParent(root.transform, false);
            wcObj.transform.localPosition = localPos;

            WheelCollider wc = wcObj.AddComponent<WheelCollider>();
            wc.radius = radius;
            wc.wheelDampingRate = 0.25f;
            wc.suspensionDistance = 0.20f;
            wc.forceAppPointDistance = 0.1f;

            // Wheel Visual Object
            GameObject visualObj = new GameObject(name + "_Visual");
            visualObj.transform.SetParent(root.transform, false);
            visualObj.transform.localPosition = localPos;

            // Tire Mesh (Cylinder)
            GameObject tireMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tireMesh.name = "TireMesh";
            tireMesh.transform.SetParent(visualObj.transform, false);
            tireMesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // align cylinder horizontally
            tireMesh.transform.localScale = new Vector3(radius * 2f, width * 0.5f, radius * 2f);

            Collider tireCol = tireMesh.GetComponent<Collider>();
            if (tireCol != null) Object.DestroyImmediate(tireCol);

            MeshRenderer tireRend = tireMesh.GetComponent<MeshRenderer>();
            if (tireRend != null) tireRend.sharedMaterial = matTire;

            // Rim Mesh (Inner Cylinder)
            GameObject rimMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rimMesh.name = "AlloyRim";
            rimMesh.transform.SetParent(visualObj.transform, false);
            rimMesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            rimMesh.transform.localScale = new Vector3(radius * 1.45f, width * 0.52f, radius * 1.45f);

            Collider rimCol = rimMesh.GetComponent<Collider>();
            if (rimCol != null) Object.DestroyImmediate(rimCol);

            MeshRenderer rimRend = rimMesh.GetComponent<MeshRenderer>();
            if (rimRend != null) rimRend.sharedMaterial = matRim;

            collider = wc;
            visual = visualObj.transform;
        }

        private static TrailRenderer CreateSkidTrail(string name, Transform parent, float radius)
        {
            GameObject trailObj = new GameObject(name);
            trailObj.transform.SetParent(parent, false);
            trailObj.transform.localPosition = new Vector3(0f, -radius * 0.9f, 0f);

            TrailRenderer tr = trailObj.AddComponent<TrailRenderer>();
            tr.time = 2.0f;
            tr.startWidth = 0.24f;
            tr.endWidth = 0.22f;
            tr.emitting = false;

            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            trailMat.color = new Color(0.1f, 0.1f, 0.1f, 0.4f);
            tr.sharedMaterial = trailMat;

            return tr;
        }

        private static Material CreateMaterial(Shader shader, string name, Color color, float smoothness, float metallic)
        {
            Material mat = new Material(shader) { name = name };
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            return mat;
        }

        private static Material CreateEmissiveMaterial(Shader shader, string name, Color baseColor, Color emissiveColor)
        {
            Material mat = CreateMaterial(shader, name, baseColor, 0.9f, 0.0f);
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissiveColor);
            return mat;
        }

        #endregion
    }
}
