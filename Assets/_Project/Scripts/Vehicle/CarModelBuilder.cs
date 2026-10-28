using UnityEngine;
using UnityEngine.Rendering;

namespace ThegameioRacing.Vehicle
{
    /// <summary>
    /// Constructs a 3D procedural vehicle body, wheels, materials, colliders,
    /// and attaches the WheelColliders and VehicleController.
    /// Produces a sleek, original urban racing car.
    /// </summary>
    public static class CarModelBuilder
    {
        public static GameObject BuildProceduralCar(string name, VehicleConfig config)
        {
            GameObject carRoot = new GameObject(name);
            carRoot.tag = "Player";

            // Rigidbody
            Rigidbody rb = carRoot.AddComponent<Rigidbody>();
            rb.mass = config != null ? config.mass : 1350f;
            rb.drag = 0.05f;
            rb.angularDrag = 2.5f;
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

            Material matBody = CreateMaterial(litShader, "Mat_CarBody", bodyCol, 0.85f, 0.75f);
            Material matAccent = CreateMaterial(litShader, "Mat_CarAccent", accentCol, 0.8f, 0.8f);
            Material matGlass = CreateMaterial(litShader, "Mat_CarGlass", new Color(0.08f, 0.1f, 0.14f), 0.95f, 0.2f);
            Material matTrim = CreateMaterial(litShader, "Mat_CarTrim", new Color(0.12f, 0.12f, 0.14f), 0.3f, 0.1f);
            Material matTire = CreateMaterial(litShader, "Mat_TireRubber", new Color(0.14f, 0.14f, 0.15f), 0.1f, 0.0f);
            Material matRim = CreateMaterial(litShader, "Mat_AlloyRim", new Color(0.75f, 0.77f, 0.82f), 0.9f, 0.9f);

            Material matHeadlights = CreateEmissiveMaterial(litShader, "Mat_Headlights", new Color(0.9f, 0.95f, 1f), new Color(1.5f, 2.0f, 2.5f));
            Material matTaillights = CreateEmissiveMaterial(litShader, "Mat_Taillights", new Color(0.85f, 0.1f, 0.1f), new Color(2.5f, 0.1f, 0.1f));

            // Chassis Visual Root
            GameObject visualRoot = new GameObject("VisualChassis");
            visualRoot.transform.SetParent(carRoot.transform, false);

            // 1. Lower Body / Tub
            CreateBoxPart("LowerBody", visualRoot.transform, new Vector3(0f, 0.42f, 0f), new Vector3(1.85f, 0.38f, 4.3f), matBody);

            // 2. Cabin / Greenhouse
            CreateBoxPart("CabinRoof", visualRoot.transform, new Vector3(0f, 0.88f, -0.2f), new Vector3(1.42f, 0.52f, 2.1f), matGlass);

            // 3. Front Hood Slope
            CreateBoxPart("FrontHood", visualRoot.transform, new Vector3(0f, 0.56f, 1.25f), new Vector3(1.72f, 0.24f, 1.4f), matBody);

            // 4. Rear Deck & Trunk
            CreateBoxPart("RearDeck", visualRoot.transform, new Vector3(0f, 0.62f, -1.45f), new Vector3(1.72f, 0.26f, 1.1f), matBody);

            // 5. Front Splitter / Bumper
            CreateBoxPart("FrontSplitter", visualRoot.transform, new Vector3(0f, 0.22f, 2.15f), new Vector3(1.88f, 0.14f, 0.35f), matTrim);

            // 6. Rear Diffuser
            CreateBoxPart("RearDiffuser", visualRoot.transform, new Vector3(0f, 0.25f, -2.15f), new Vector3(1.82f, 0.18f, 0.3f), matTrim);

            // 7. Side Skirts
            CreateBoxPart("SideSkirt_L", visualRoot.transform, new Vector3(-0.93f, 0.24f, 0f), new Vector3(0.08f, 0.14f, 2.5f), matTrim);
            CreateBoxPart("SideSkirt_R", visualRoot.transform, new Vector3(0.93f, 0.24f, 0f), new Vector3(0.08f, 0.14f, 2.5f), matTrim);

            // 8. Rear Spoiler
            GameObject spoiler = new GameObject("RearSpoiler");
            spoiler.transform.SetParent(visualRoot.transform, false);
            CreateBoxPart("SpoilerWing", spoiler.transform, new Vector3(0f, 0.98f, -1.92f), new Vector3(1.68f, 0.06f, 0.36f), matAccent);
            CreateBoxPart("SpoilerPillar_L", spoiler.transform, new Vector3(-0.55f, 0.82f, -1.90f), new Vector3(0.05f, 0.28f, 0.15f), matTrim);
            CreateBoxPart("SpoilerPillar_R", spoiler.transform, new Vector3(0.55f, 0.82f, -1.90f), new Vector3(0.05f, 0.28f, 0.15f), matTrim);

            // 9. Headlights & Taillights
            CreateBoxPart("Headlight_L", visualRoot.transform, new Vector3(-0.65f, 0.52f, 2.14f), new Vector3(0.38f, 0.10f, 0.08f), matHeadlights);
            CreateBoxPart("Headlight_R", visualRoot.transform, new Vector3(0.65f, 0.52f, 2.14f), new Vector3(0.38f, 0.10f, 0.08f), matHeadlights);
            CreateBoxPart("Taillight_L", visualRoot.transform, new Vector3(-0.65f, 0.62f, -2.14f), new Vector3(0.48f, 0.10f, 0.08f), matTaillights);
            CreateBoxPart("Taillight_R", visualRoot.transform, new Vector3(0.65f, 0.62f, -2.14f), new Vector3(0.48f, 0.10f, 0.08f), matTaillights);

            // Body Collider (Composite BoxColliders on carRoot)
            BoxCollider bodyCollider = carRoot.AddComponent<BoxCollider>();
            bodyCollider.center = new Vector3(0f, 0.55f, 0f);
            bodyCollider.size = new Vector3(1.86f, 0.75f, 4.25f);

            // Wheel Positions
            float frontZ = 1.35f;
            float rearZ = -1.35f;
            float trackHalfWidth = 0.86f;
            float wheelY = 0.36f;
            float wheelRadius = 0.36f;
            float wheelWidth = 0.28f;

            // Setup Wheels
            CreateWheel(carRoot, "Wheel_FL", new Vector3(-trackHalfWidth, wheelY, frontZ), wheelRadius, wheelWidth, matTire, matRim,
                out WheelCollider flC, out Transform flV);
            CreateWheel(carRoot, "Wheel_FR", new Vector3(trackHalfWidth, wheelY, frontZ), wheelRadius, wheelWidth, matTire, matRim,
                out WheelCollider frC, out Transform frV);
            CreateWheel(carRoot, "Wheel_RL", new Vector3(-trackHalfWidth, wheelY, rearZ), wheelRadius, wheelWidth, matTire, matRim,
                out WheelCollider rlC, out Transform rlV);
            CreateWheel(carRoot, "Wheel_RR", new Vector3(trackHalfWidth, wheelY, rearZ), wheelRadius, wheelWidth, matTire, matRim,
                out WheelCollider rrC, out Transform rrV);

            controller.SetupComponents(flC, frC, rlC, rrC, flV, frV, rlV, rrV);

            // Skid trails
            TrailRenderer[] trails = new TrailRenderer[2];
            trails[0] = CreateSkidTrail("SkidTrail_RL", rlV);
            trails[1] = CreateSkidTrail("SkidTrail_RR", rrV);

            return carRoot;
        }

        private static void CreateBoxPart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            // Remove default collider so it doesn't interfere with chassis BoxCollider / WheelColliders
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

        private static TrailRenderer CreateSkidTrail(string name, Transform parent)
        {
            GameObject trailObj = new GameObject(name);
            trailObj.transform.SetParent(parent, false);
            trailObj.transform.localPosition = new Vector3(0f, -0.32f, 0f);

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
    }
}
