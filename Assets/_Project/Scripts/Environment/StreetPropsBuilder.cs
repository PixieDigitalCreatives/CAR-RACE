using UnityEngine;

namespace ThegameioRacing.Environment
{
    /// <summary>
    /// Procedural 3D builder for representative African coastal metropolis street components.
    /// Produces modular road segments, sidewalks, curbs, storefronts, commercial towers,
    /// streetlights, race barriers, billboards, and spectators.
    /// </summary>
    public static class StreetPropsBuilder
    {
        // =================================================================================
        // 1. ROADWAY, CURBS & SIDEWALKS
        // =================================================================================

        public static GameObject CreateRoadAvenue(
            Transform parent,
            Material matAsphalt,
            Material matPavement,
            Material matCurb,
            float length = 120f,
            float roadWidth = 14f,
            float sidewalkWidth = 3.5f,
            float curbHeight = 0.18f)
        {
            GameObject roadRoot = new GameObject("Avenue_Section");
            roadRoot.transform.SetParent(parent, false);

            // 1. Driving Asphalt Surface
            GameObject asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            asphalt.name = "Road_Asphalt_Surface";
            asphalt.transform.SetParent(roadRoot.transform, false);
            asphalt.transform.position = new Vector3(0f, -0.05f, length * 0.5f);
            asphalt.transform.localScale = new Vector3(roadWidth, 0.1f, length);
            if (matAsphalt != null) asphalt.GetComponent<Renderer>().sharedMaterial = matAsphalt;

            // 2. Left Curb & Sidewalk
            float leftCurbX = -(roadWidth * 0.5f + 0.15f);
            float leftSidewalkX = -(roadWidth * 0.5f + 0.3f + sidewalkWidth * 0.5f);

            GameObject leftCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftCurb.name = "Curb_Left";
            leftCurb.transform.SetParent(roadRoot.transform, false);
            leftCurb.transform.position = new Vector3(leftCurbX, curbHeight * 0.5f, length * 0.5f);
            leftCurb.transform.localScale = new Vector3(0.3f, curbHeight, length);
            if (matCurb != null) leftCurb.GetComponent<Renderer>().sharedMaterial = matCurb;

            GameObject leftSidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftSidewalk.name = "Sidewalk_Left";
            leftSidewalk.transform.SetParent(roadRoot.transform, false);
            leftSidewalk.transform.position = new Vector3(leftSidewalkX, (curbHeight - 0.02f) * 0.5f, length * 0.5f);
            leftSidewalk.transform.localScale = new Vector3(sidewalkWidth, curbHeight - 0.02f, length);
            if (matPavement != null) leftSidewalk.GetComponent<Renderer>().sharedMaterial = matPavement;

            // 3. Right Curb & Sidewalk
            float rightCurbX = (roadWidth * 0.5f + 0.15f);
            float rightSidewalkX = (roadWidth * 0.5f + 0.3f + sidewalkWidth * 0.5f);

            GameObject rightCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightCurb.name = "Curb_Right";
            rightCurb.transform.SetParent(roadRoot.transform, false);
            rightCurb.transform.position = new Vector3(rightCurbX, curbHeight * 0.5f, length * 0.5f);
            rightCurb.transform.localScale = new Vector3(0.3f, curbHeight, length);
            if (matCurb != null) rightCurb.GetComponent<Renderer>().sharedMaterial = matCurb;

            GameObject rightSidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightSidewalk.name = "Sidewalk_Right";
            rightSidewalk.transform.SetParent(roadRoot.transform, false);
            rightSidewalk.transform.position = new Vector3(rightSidewalkX, (curbHeight - 0.02f) * 0.5f, length * 0.5f);
            rightSidewalk.transform.localScale = new Vector3(sidewalkWidth, curbHeight - 0.02f, length);
            if (matPavement != null) rightSidewalk.GetComponent<Renderer>().sharedMaterial = matPavement;

            return roadRoot;
        }

        // =================================================================================
        // 2. ROAD MARKINGS
        // =================================================================================

        public static void CreateRoadMarkings(
            Transform parent,
            Material matWhite,
            Material matYellow,
            float length = 120f,
            float roadWidth = 14f)
        {
            GameObject markingsRoot = new GameObject("Road_Markings");
            markingsRoot.transform.SetParent(parent, false);

            float halfRoad = roadWidth * 0.5f;
            float laneWidth = roadWidth * 0.25f; // 4 lanes total, 3.5m each

            // A. Center Double Yellow Line
            float yellowSpacing = 0.12f;
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject centerLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
                centerLine.name = $"Center_Yellow_{i}";
                centerLine.transform.SetParent(markingsRoot.transform, false);
                centerLine.transform.position = new Vector3(i * yellowSpacing, 0.005f, length * 0.5f);
                centerLine.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                centerLine.transform.localScale = new Vector3(0.12f, length, 1f);
                Object.DestroyImmediate(centerLine.GetComponent<Collider>());
                if (matYellow != null) centerLine.GetComponent<Renderer>().sharedMaterial = matYellow;
            }

            // B. Dashed White Lane Dividers
            float dashLen = 3.5f;
            float gapLen = 5.5f;
            float stride = dashLen + gapLen;
            int dashCount = Mathf.FloorToInt(length / stride);

            // Left lane divider (-laneWidth) and Right lane divider (+laneWidth)
            float[] laneDividers = new float[] { -laneWidth, laneWidth };
            foreach (float lx in laneDividers)
            {
                for (int d = 0; d < dashCount; d++)
                {
                    float z = d * stride + dashLen * 0.5f;
                    GameObject dash = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    dash.name = $"Lane_Dash_{lx:F1}_{d}";
                    dash.transform.SetParent(markingsRoot.transform, false);
                    dash.transform.position = new Vector3(lx, 0.005f, z);
                    dash.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    dash.transform.localScale = new Vector3(0.15f, dashLen, 1f);
                    Object.DestroyImmediate(dash.GetComponent<Collider>());
                    if (matWhite != null) dash.GetComponent<Renderer>().sharedMaterial = matWhite;
                }
            }

            // C. Solid White Shoulder Lines
            float shoulderOffset = halfRoad - 0.35f;
            float[] shoulders = new float[] { -shoulderOffset, shoulderOffset };
            foreach (float sx in shoulders)
            {
                GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Quad);
                shoulder.name = $"Shoulder_Line_{sx:F1}";
                shoulder.transform.SetParent(markingsRoot.transform, false);
                shoulder.transform.position = new Vector3(sx, 0.005f, length * 0.5f);
                shoulder.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                shoulder.transform.localScale = new Vector3(0.18f, length, 1f);
                Object.DestroyImmediate(shoulder.GetComponent<Collider>());
                if (matWhite != null) shoulder.GetComponent<Renderer>().sharedMaterial = matWhite;
            }

            // D. Starting Grid Stalls (2 columns x 5 rows as specified in PPT)
            float startZ = 15f;
            float gridRowSpacing = 8.5f;
            float colLeft = -laneWidth * 0.5f;
            float colRight = laneWidth * 0.5f;

            for (int row = 0; row < 5; row++)
            {
                float rz = startZ + row * gridRowSpacing;

                // Left Stall Box
                CreateGridStall(markingsRoot.transform, new Vector3(colLeft, 0.006f, rz), (row * 2 + 1), matWhite, matYellow);

                // Right Stall Box
                CreateGridStall(markingsRoot.transform, new Vector3(colRight, 0.006f, rz + 2.5f), (row * 2 + 2), matWhite, matYellow);
            }
        }

        private static void CreateGridStall(Transform parent, Vector3 pos, int slotNum, Material matWhite, Material matYellow)
        {
            GameObject stall = new GameObject($"Grid_Slot_{slotNum:D2}");
            stall.transform.SetParent(parent, false);
            stall.transform.position = pos;

            // Transverse start bar
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bar.name = "Stall_Bar";
            bar.transform.SetParent(stall.transform, false);
            bar.transform.localPosition = Vector3.zero;
            bar.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            bar.transform.localScale = new Vector3(2.2f, 0.25f, 1f);
            Object.DestroyImmediate(bar.GetComponent<Collider>());
            if (matYellow != null) bar.GetComponent<Renderer>().sharedMaterial = matYellow;

            // Side tick left
            GameObject tickL = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tickL.name = "Tick_Left";
            tickL.transform.SetParent(stall.transform, false);
            tickL.transform.localPosition = new Vector3(-1.1f, 0f, 0.6f);
            tickL.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            tickL.transform.localScale = new Vector3(0.12f, 1.2f, 1f);
            Object.DestroyImmediate(tickL.GetComponent<Collider>());
            if (matWhite != null) tickL.GetComponent<Renderer>().sharedMaterial = matWhite;

            // Side tick right
            GameObject tickR = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tickR.name = "Tick_Right";
            tickR.transform.SetParent(stall.transform, false);
            tickR.transform.localPosition = new Vector3(1.1f, 0f, 0.6f);
            tickR.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            tickR.transform.localScale = new Vector3(0.12f, 1.2f, 1f);
            Object.DestroyImmediate(tickR.GetComponent<Collider>());
            if (matWhite != null) tickR.GetComponent<Renderer>().sharedMaterial = matWhite;
        }

        // =================================================================================
        // 3. STREETLIGHT WITH WARM SODIUM CONE
        // =================================================================================

        public static GameObject CreateStreetlight(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matMetal,
            Material matBulb)
        {
            GameObject lightRoot = new GameObject("Streetlight_Sodium");
            lightRoot.transform.SetParent(parent, false);
            lightRoot.transform.position = position;
            lightRoot.transform.rotation = rotation;

            float poleHeight = 7.5f;

            // Vertical Mast
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Pole_Mast";
            mast.transform.SetParent(lightRoot.transform, false);
            mast.transform.localPosition = new Vector3(0f, poleHeight * 0.5f, 0f);
            mast.transform.localScale = new Vector3(0.18f, poleHeight * 0.5f, 0.18f);
            if (matMetal != null) mast.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Base Ring
            GameObject baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseRing.name = "Pole_Base";
            baseRing.transform.SetParent(lightRoot.transform, false);
            baseRing.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            baseRing.transform.localScale = new Vector3(0.45f, 0.25f, 0.45f);
            if (matMetal != null) baseRing.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Curved Luminaire Overhang Arm
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "Pole_Arm";
            arm.transform.SetParent(lightRoot.transform, false);
            arm.transform.localPosition = new Vector3(1.1f, poleHeight - 0.2f, 0f);
            arm.transform.localRotation = Quaternion.Euler(0f, 0f, -75f);
            arm.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
            if (matMetal != null) arm.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Cobra-head Fixture
            GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fixture.name = "Lamp_Fixture";
            fixture.transform.SetParent(lightRoot.transform, false);
            fixture.transform.localPosition = new Vector3(2.1f, poleHeight - 0.1f, 0f);
            fixture.transform.localScale = new Vector3(0.9f, 0.22f, 0.45f);
            if (matMetal != null) fixture.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Emissive Bulb Face
            GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bulb.name = "Bulb_Emissive";
            bulb.transform.SetParent(fixture.transform, false);
            bulb.transform.localPosition = new Vector3(0f, -0.52f, 0f);
            bulb.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            bulb.transform.localScale = new Vector3(0.75f, 0.35f, 1f);
            Object.DestroyImmediate(bulb.GetComponent<Collider>());
            if (matBulb != null) bulb.GetComponent<Renderer>().sharedMaterial = matBulb;

            // Downward Warm Sodium Spotlight
            GameObject lightObj = new GameObject("Sodium_Spotlight");
            lightObj.transform.SetParent(fixture.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            lightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Light spot = lightObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1.0f, 0.72f, 0.42f); // Warm sodium 3000K
            spot.intensity = 28f;
            spot.range = 22f;
            spot.spotAngle = 72f;
            spot.innerSpotAngle = 45f;
            spot.shadows = LightShadows.Soft;

            return lightRoot;
        }

        // =================================================================================
        // 4. GROUND-FLOOR AFRICAN CITY STOREFRONT
        // =================================================================================

        public static GameObject CreateStorefront(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matPlaster,
            Material matSign,
            Material matAwning,
            Material matGlass,
            Material matMetal,
            string storeName = "KILIMA ELECTRONICS")
        {
            GameObject shopRoot = new GameObject($"Storefront_{storeName}");
            shopRoot.transform.SetParent(parent, false);
            shopRoot.transform.position = position;
            shopRoot.transform.rotation = rotation;

            float width = 12f;
            float height = 5.2f;
            float depth = 8f;

            // Main Building Shell
            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shell.name = "Store_MainShell";
            shell.transform.SetParent(shopRoot.transform, false);
            shell.transform.localPosition = new Vector3(0f, height * 0.5f, depth * 0.5f);
            shell.transform.localScale = new Vector3(width, height, depth);
            if (matPlaster != null) shell.GetComponent<Renderer>().sharedMaterial = matPlaster;

            // Recessed Front Facia Frame
            GameObject fascia = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fascia.name = "Store_Fascia";
            fascia.transform.SetParent(shopRoot.transform, false);
            fascia.transform.localPosition = new Vector3(0f, height - 0.7f, -0.1f);
            fascia.transform.localScale = new Vector3(width * 0.94f, 1.4f, 0.35f);
            if (matPlaster != null) fascia.GetComponent<Renderer>().sharedMaterial = matPlaster;

            // Illuminated Storefront Sign
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sign.name = "Sign_Board";
            sign.transform.SetParent(fascia.transform, false);
            sign.transform.localPosition = new Vector3(0f, 0f, -0.52f);
            sign.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            sign.transform.localScale = new Vector3(0.92f, 0.75f, 1f);
            Object.DestroyImmediate(sign.GetComponent<Collider>());
            if (matSign != null) sign.GetComponent<Renderer>().sharedMaterial = matSign;

            // Sign Backlight Spot
            GameObject signLight = new GameObject("Sign_Backlight");
            signLight.transform.SetParent(sign.transform, false);
            signLight.transform.localPosition = new Vector3(0f, 0f, 0.4f);
            Light sLight = signLight.AddComponent<Light>();
            sLight.type = LightType.Point;
            sLight.color = new Color(1.0f, 0.85f, 0.6f);
            sLight.intensity = 8f;
            sLight.range = 6f;

            // Glass Display Windows (Left and Right of entrance)
            float windowW = 3.6f;
            float windowH = 2.4f;
            float windowY = 1.4f;

            // Left Display Window
            GameObject winL = GameObject.CreatePrimitive(PrimitiveType.Quad);
            winL.name = "DisplayWindow_Left";
            winL.transform.SetParent(shopRoot.transform, false);
            winL.transform.localPosition = new Vector3(-3.2f, windowY, -0.02f);
            winL.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            winL.transform.localScale = new Vector3(windowW, windowH, 1f);
            if (matGlass != null) winL.GetComponent<Renderer>().sharedMaterial = matGlass;

            // Right Display Window
            GameObject winR = GameObject.CreatePrimitive(PrimitiveType.Quad);
            winR.name = "DisplayWindow_Right";
            winR.transform.SetParent(shopRoot.transform, false);
            winR.transform.localPosition = new Vector3(3.2f, windowY, -0.02f);
            winR.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            winR.transform.localScale = new Vector3(windowW, windowH, 1f);
            if (matGlass != null) winR.GetComponent<Renderer>().sharedMaterial = matGlass;

            // Central Entrance Door Frame
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Entrance_Door";
            door.transform.SetParent(shopRoot.transform, false);
            door.transform.localPosition = new Vector3(0f, 1.4f, 0.3f);
            door.transform.localScale = new Vector3(2.4f, 2.6f, 0.2f);
            if (matMetal != null) door.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Warm Interior Shop Glow Light
            GameObject interiorLight = new GameObject("Interior_Warm_Glow");
            interiorLight.transform.SetParent(shopRoot.transform, false);
            interiorLight.transform.localPosition = new Vector3(0f, 2.4f, 2.5f);
            Light inLight = interiorLight.AddComponent<Light>();
            inLight.type = LightType.Point;
            inLight.color = new Color(1.0f, 0.78f, 0.45f); // Warm interior gold
            inLight.intensity = 16f;
            inLight.range = 10f;
            inLight.shadows = LightShadows.None;

            // Slanted Striped Fabric Awning
            GameObject awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            awning.name = "Awning_Fabric";
            awning.transform.SetParent(shopRoot.transform, false);
            awning.transform.localPosition = new Vector3(0f, 3.2f, -1.0f);
            awning.transform.localRotation = Quaternion.Euler(16f, 0f, 0f);
            awning.transform.localScale = new Vector3(width * 0.88f, 0.08f, 2.2f);
            if (matAwning != null) awning.GetComponent<Renderer>().sharedMaterial = matAwning;

            // Metal Awning Stay Bars
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject stay = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stay.name = $"Awning_Stay_{i}";
                stay.transform.SetParent(shopRoot.transform, false);
                stay.transform.localPosition = new Vector3(i * (width * 0.42f), 2.7f, -1.0f);
                stay.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
                stay.transform.localScale = new Vector3(0.04f, 1.2f, 0.04f);
                if (matMetal != null) stay.GetComponent<Renderer>().sharedMaterial = matMetal;
            }

            return shopRoot;
        }

        // =================================================================================
        // 5. COMMERCIAL / RESIDENTIAL TOWER WITH LIT WINDOWS & MURAL
        // =================================================================================

        public static GameObject CreateCommercialBuilding(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matFacade,
            Material matWindows,
            Material matMural,
            Material matMetal,
            bool hasMural = true,
            float width = 18f,
            float height = 22f,
            float depth = 16f)
        {
            GameObject bldRoot = new GameObject("Commercial_Tower");
            bldRoot.transform.SetParent(parent, false);
            bldRoot.transform.position = position;
            bldRoot.transform.rotation = rotation;

            // Main Tower Block
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "Tower_MainBlock";
            block.transform.SetParent(bldRoot.transform, false);
            block.transform.localPosition = new Vector3(0f, height * 0.5f, depth * 0.5f);
            block.transform.localScale = new Vector3(width, height, depth);
            if (matFacade != null) block.GetComponent<Renderer>().sharedMaterial = matFacade;

            // Rooftop Parapet Rim
            GameObject parapet = GameObject.CreatePrimitive(PrimitiveType.Cube);
            parapet.name = "Rooftop_Parapet";
            parapet.transform.SetParent(bldRoot.transform, false);
            parapet.transform.localPosition = new Vector3(0f, height + 0.45f, depth * 0.5f);
            parapet.transform.localScale = new Vector3(width + 0.3f, 0.9f, depth + 0.3f);
            if (matFacade != null) parapet.GetComponent<Renderer>().sharedMaterial = matFacade;

            // Front Window Array (Illuminated / Dark windows matrix)
            GameObject winFront = GameObject.CreatePrimitive(PrimitiveType.Quad);
            winFront.name = "Windows_FrontFacade";
            winFront.transform.SetParent(bldRoot.transform, false);
            winFront.transform.localPosition = new Vector3(0f, height * 0.55f, -0.05f);
            winFront.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            winFront.transform.localScale = new Vector3(width * 0.88f, height * 0.72f, 1f);
            Object.DestroyImmediate(winFront.GetComponent<Collider>());
            if (matWindows != null) winFront.GetComponent<Renderer>().sharedMaterial = matWindows;

            // Second & Third Floor Balconies
            float[] balconyHeights = new float[] { 7.0f, 12.5f, 18.0f };
            foreach (float by in balconyHeights)
            {
                GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slab.name = $"Balcony_Slab_{by:F0}";
                slab.transform.SetParent(bldRoot.transform, false);
                slab.transform.localPosition = new Vector3(0f, by, -0.6f);
                slab.transform.localScale = new Vector3(width * 0.85f, 0.25f, 1.2f);
                if (matFacade != null) slab.GetComponent<Renderer>().sharedMaterial = matFacade;

                // Railing
                GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = $"Balcony_Rail_{by:F0}";
                rail.transform.SetParent(bldRoot.transform, false);
                rail.transform.localPosition = new Vector3(0f, by + 0.55f, -1.15f);
                rail.transform.localScale = new Vector3(width * 0.85f, 0.85f, 0.08f);
                if (matMetal != null) rail.GetComponent<Renderer>().sharedMaterial = matMetal;
            }

            // African Cultural Wall Mural on Side Flank
            if (hasMural && matMural != null)
            {
                GameObject mural = GameObject.CreatePrimitive(PrimitiveType.Quad);
                mural.name = "Side_Cultural_Mural";
                mural.transform.SetParent(bldRoot.transform, false);
                // Placed on left exterior flank
                mural.transform.localPosition = new Vector3(-width * 0.5f - 0.05f, height * 0.5f, depth * 0.5f);
                mural.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                mural.transform.localScale = new Vector3(depth * 0.65f, height * 0.65f, 1f);
                Object.DestroyImmediate(mural.GetComponent<Collider>());
                mural.GetComponent<Renderer>().sharedMaterial = matMural;

                // Mural Illumination Spotlight
                GameObject mLightObj = new GameObject("Mural_Spotlight");
                mLightObj.transform.SetParent(bldRoot.transform, false);
                mLightObj.transform.localPosition = new Vector3(-width * 0.5f - 2.5f, height * 0.85f, depth * 0.5f);
                mLightObj.transform.localRotation = Quaternion.Euler(45f, 90f, 0f);

                Light mLight = mLightObj.AddComponent<Light>();
                mLight.type = LightType.Spot;
                mLight.color = new Color(1.0f, 0.9f, 0.7f);
                mLight.intensity = 15f;
                mLight.range = 14f;
                mLight.spotAngle = 60f;
            }

            return bldRoot;
        }

        // =================================================================================
        // 6. SAFETY BARRIER WITH CHEVRON HAZARD PATTERN
        // =================================================================================

        public static GameObject CreateSafetyBarrier(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matMetal,
            Material matChevron,
            float length = 4f)
        {
            GameObject barrierRoot = new GameObject("Safety_Barrier");
            barrierRoot.transform.SetParent(parent, false);
            barrierRoot.transform.position = position;
            barrierRoot.transform.rotation = rotation;

            float barrierH = 0.95f;
            float barrierW = 0.35f;

            // Concrete / Steel Crash Barrier Base
            GameObject baseBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseBlock.name = "Barrier_Base";
            baseBlock.transform.SetParent(barrierRoot.transform, false);
            baseBlock.transform.localPosition = new Vector3(0f, barrierH * 0.45f, 0f);
            baseBlock.transform.localScale = new Vector3(barrierW, barrierH * 0.9f, length);
            if (matChevron != null) baseBlock.GetComponent<Renderer>().sharedMaterial = matChevron;

            // Heavy Steel Top Rail
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rail.name = "Barrier_TopRail";
            rail.transform.SetParent(barrierRoot.transform, false);
            rail.transform.localPosition = new Vector3(0f, barrierH + 0.08f, 0f);
            rail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rail.transform.localScale = new Vector3(0.12f, length * 0.5f, 0.12f);
            if (matMetal != null) rail.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Reflective Warning Light on End Post
            GameObject lightOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lightOrb.name = "Reflective_Marker";
            lightOrb.transform.SetParent(barrierRoot.transform, false);
            lightOrb.transform.localPosition = new Vector3(0f, barrierH + 0.18f, length * 0.45f);
            lightOrb.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            Object.DestroyImmediate(lightOrb.GetComponent<Collider>());
            if (matChevron != null) lightOrb.GetComponent<Renderer>().sharedMaterial = matChevron;

            return barrierRoot;
        }

        // =================================================================================
        // 7. URBAN BILLBOARD WITH GANTRY FRAME
        // =================================================================================

        public static GameObject CreateUrbanBillboard(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matSteel,
            Material matPoster,
            float boardWidth = 10f,
            float boardHeight = 5f,
            float postHeight = 6.5f)
        {
            GameObject boardRoot = new GameObject("Urban_Billboard");
            boardRoot.transform.SetParent(parent, false);
            boardRoot.transform.position = position;
            boardRoot.transform.rotation = rotation;

            // Twin Steel Support Columns
            float colSpacing = boardWidth * 0.35f;
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                col.name = $"Support_Column_{i}";
                col.transform.SetParent(boardRoot.transform, false);
                col.transform.localPosition = new Vector3(i * colSpacing, postHeight * 0.5f, 0f);
                col.transform.localScale = new Vector3(0.35f, postHeight * 0.5f, 0.35f);
                if (matSteel != null) col.GetComponent<Renderer>().sharedMaterial = matSteel;
            }

            // Billboard Back Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Board_Frame";
            frame.transform.SetParent(boardRoot.transform, false);
            frame.transform.localPosition = new Vector3(0f, postHeight + boardHeight * 0.5f, 0f);
            frame.transform.localScale = new Vector3(boardWidth, boardHeight, 0.4f);
            if (matSteel != null) frame.GetComponent<Renderer>().sharedMaterial = matSteel;

            // Poster Face Quad
            GameObject poster = GameObject.CreatePrimitive(PrimitiveType.Quad);
            poster.name = "Poster_Graphic";
            poster.transform.SetParent(frame.transform, false);
            poster.transform.localPosition = new Vector3(0f, 0f, -0.52f);
            poster.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            poster.transform.localScale = new Vector3(0.96f, 0.94f, 1f);
            Object.DestroyImmediate(poster.GetComponent<Collider>());
            if (matPoster != null) poster.GetComponent<Renderer>().sharedMaterial = matPoster;

            // Overhead Floodlights
            for (int f = -1; f <= 1; f += 2)
            {
                GameObject floodlight = new GameObject($"Billboard_Floodlight_{f}");
                floodlight.transform.SetParent(boardRoot.transform, false);
                floodlight.transform.localPosition = new Vector3(f * (boardWidth * 0.3f), postHeight + boardHeight + 0.8f, -1.2f);
                floodlight.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);

                Light flood = floodlight.AddComponent<Light>();
                flood.type = LightType.Spot;
                flood.color = new Color(1.0f, 0.92f, 0.8f);
                flood.intensity = 18f;
                flood.range = 10f;
                flood.spotAngle = 75f;
            }

            return boardRoot;
        }

        // =================================================================================
        // 8. SPECTATOR CROWD CLUSTER (SAFE SIDEWALK PLACEMENT)
        // =================================================================================

        public static GameObject CreateSpectatorCluster(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            int count = 5)
        {
            GameObject crowdRoot = new GameObject("Spectator_Group");
            crowdRoot.transform.SetParent(parent, false);
            crowdRoot.transform.position = position;
            crowdRoot.transform.rotation = rotation;

            Color[] shirtColors = new Color[]
            {
                new Color(0.85f, 0.25f, 0.18f), // Vibrant Red
                new Color(0.95f, 0.75f, 0.15f), // Golden Yellow
                new Color(0.15f, 0.55f, 0.85f), // Cyan Blue
                new Color(0.22f, 0.72f, 0.35f), // Emerald Green
                new Color(0.88f, 0.88f, 0.92f)  // White
            };

            for (int i = 0; i < count; i++)
            {
                float ox = (i - count * 0.5f) * 0.75f;
                float oz = (i % 2 == 0) ? 0.3f : -0.3f;
                float height = Random.Range(1.68f, 1.84f);

                GameObject person = new GameObject($"Spectator_{i + 1}");
                person.transform.SetParent(crowdRoot.transform, false);
                person.transform.localPosition = new Vector3(ox, 0f, oz);

                // Body / Torso
                GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                torso.name = "Torso";
                torso.transform.SetParent(person.transform, false);
                torso.transform.localPosition = new Vector3(0f, height * 0.6f, 0f);
                torso.transform.localScale = new Vector3(0.42f, height * 0.38f, 0.28f);
                Object.DestroyImmediate(torso.GetComponent<Collider>());

                Renderer rend = torso.GetComponent<Renderer>();
                Material matPerson = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                matPerson.SetColor("_BaseColor", shirtColors[i % shirtColors.Length]);
                matPerson.SetFloat("_Smoothness", 0.15f);
                rend.sharedMaterial = matPerson;

                // Head
                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Head";
                head.transform.SetParent(person.transform, false);
                head.transform.localPosition = new Vector3(0f, height * 0.92f, 0f);
                head.transform.localScale = new Vector3(0.22f, 0.26f, 0.22f);
                Object.DestroyImmediate(head.GetComponent<Collider>());

                Material matSkin = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                matSkin.SetColor("_BaseColor", new Color(0.28f, 0.17f, 0.11f)); // Deep warm skin tone
                matSkin.SetFloat("_Smoothness", 0.25f);
                head.GetComponent<Renderer>().sharedMaterial = matSkin;
            }

            return crowdRoot;
        }

        // =================================================================================
        // 9. VERTICAL ILLUMINATED NEON BLADE SIGN ("AFRICA")
        // =================================================================================

        public static GameObject CreateVerticalBladeSign(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matSteel,
            Material matNeon,
            float width = 1.4f,
            float height = 7.5f,
            float depth = 0.25f)
        {
            GameObject signRoot = new GameObject("Vertical_Neon_Sign_Africa");
            signRoot.transform.SetParent(parent, false);
            signRoot.transform.position = position;
            signRoot.transform.rotation = rotation;

            // Cantilever Wall Mount Brackets
            float[] bracketHeights = new float[] { height * 0.2f, height * 0.8f };
            foreach (float by in bracketHeights)
            {
                GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                bracket.name = $"Mount_Bracket_{by:F0}";
                bracket.transform.SetParent(signRoot.transform, false);
                bracket.transform.localPosition = new Vector3(-width * 0.5f - 0.45f, by, 0f);
                bracket.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                bracket.transform.localScale = new Vector3(0.08f, 0.45f, 0.08f);
                if (matSteel != null) bracket.GetComponent<Renderer>().sharedMaterial = matSteel;
            }

            // Blade Housing Box
            GameObject bladeBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bladeBox.name = "Blade_Housing";
            bladeBox.transform.SetParent(signRoot.transform, false);
            bladeBox.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            bladeBox.transform.localScale = new Vector3(width, height, depth);
            if (matSteel != null) bladeBox.GetComponent<Renderer>().sharedMaterial = matSteel;

            // Front Neon Quad (faces oncoming traffic / World -Z when oriented along street)
            GameObject neonFront = GameObject.CreatePrimitive(PrimitiveType.Quad);
            neonFront.name = "Neon_Face_Front";
            neonFront.transform.SetParent(bladeBox.transform, false);
            neonFront.transform.localPosition = new Vector3(0f, 0f, -0.52f);
            neonFront.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            neonFront.transform.localScale = new Vector3(0.92f, 0.95f, 1f);
            Object.DestroyImmediate(neonFront.GetComponent<Collider>());
            if (matNeon != null) neonFront.GetComponent<Renderer>().sharedMaterial = matNeon;

            // Back Neon Quad
            GameObject neonBack = GameObject.CreatePrimitive(PrimitiveType.Quad);
            neonBack.name = "Neon_Face_Back";
            neonBack.transform.SetParent(bladeBox.transform, false);
            neonBack.transform.localPosition = new Vector3(0f, 0f, 0.52f);
            neonBack.transform.localRotation = Quaternion.identity;
            neonBack.transform.localScale = new Vector3(0.92f, 0.95f, 1f);
            Object.DestroyImmediate(neonBack.GetComponent<Collider>());
            if (matNeon != null) neonBack.GetComponent<Renderer>().sharedMaterial = matNeon;

            // Vibrant Neon Glow Point Light
            GameObject glowLight = new GameObject("Neon_Glow_Light");
            glowLight.transform.SetParent(signRoot.transform, false);
            glowLight.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);

            Light glow = glowLight.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(0.95f, 0.25f, 0.85f); // Vivid magenta / neon violet
            glow.intensity = 14f;
            glow.range = 10f;

            return signRoot;
        }

        // =================================================================================
        // 10. COASTAL PALM TREE (WATERFRONT PROMENADE)
        // =================================================================================

        public static GameObject CreatePalmTree(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matBark = null,
            Material matFronds = null,
            float height = 9f)
        {
            GameObject palmRoot = new GameObject("Coastal_PalmTree");
            palmRoot.transform.SetParent(parent, false);
            palmRoot.transform.position = position;
            palmRoot.transform.rotation = rotation;

            // Curved trunk segments
            float segH = height * 0.35f;
            Vector3[] trunkOffsets = new Vector3[]
            {
                new Vector3(0f, segH * 0.5f, 0f),
                new Vector3(0.25f, segH * 1.4f, 0.1f),
                new Vector3(0.65f, segH * 2.2f, 0.25f)
            };
            Vector3[] trunkAngles = new Vector3[]
            {
                new Vector3(2f, 0f, -4f),
                new Vector3(4f, 0f, -8f),
                new Vector3(6f, 0f, -12f)
            };

            for (int i = 0; i < 3; i++)
            {
                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                seg.name = $"Trunk_Seg_{i}";
                seg.transform.SetParent(palmRoot.transform, false);
                seg.transform.localPosition = trunkOffsets[i];
                seg.transform.localRotation = Quaternion.Euler(trunkAngles[i]);
                float taper = 0.42f - (i * 0.08f);
                seg.transform.localScale = new Vector3(taper, segH * 0.5f, taper);
                if (matBark != null) seg.GetComponent<Renderer>().sharedMaterial = matBark;
            }

            // Crown fronds
            Vector3 crownPos = trunkOffsets[2] + new Vector3(0.2f, segH * 0.5f, 0.1f);
            for (int f = 0; f < 8; f++)
            {
                float angle = f * 45f;
                GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frond.name = $"Frond_{f}";
                frond.transform.SetParent(palmRoot.transform, false);
                frond.transform.localPosition = crownPos + Quaternion.Euler(0f, angle, 0f) * new Vector3(1.2f, -0.2f, 0f);
                frond.transform.localRotation = Quaternion.Euler(20f, angle, 0f);
                frond.transform.localScale = new Vector3(2.6f, 0.08f, 0.75f);
                Object.DestroyImmediate(frond.GetComponent<Collider>());
                if (matFronds != null) frond.GetComponent<Renderer>().sharedMaterial = matFronds;
            }

            return palmRoot;
        }

        // =================================================================================
        // 11. COASTAL SEAWALL & OCEAN WATER PLANE
        // =================================================================================

        public static GameObject CreateCoastalSeawall(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            float length,
            Material matStone,
            Material matMetal)
        {
            GameObject wallRoot = new GameObject("Coastal_Seawall");
            wallRoot.transform.SetParent(parent, false);
            wallRoot.transform.position = position;
            wallRoot.transform.rotation = rotation;

            float wallH = 1.6f;
            float wallW = 0.85f;

            // Concrete seawall foundation block
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Seawall_Block";
            wall.transform.SetParent(wallRoot.transform, false);
            wall.transform.localPosition = new Vector3(0f, wallH * 0.5f, 0f);
            wall.transform.localScale = new Vector3(wallW, wallH, length);
            if (matStone != null) wall.GetComponent<Renderer>().sharedMaterial = matStone;

            // Stainless Steel Promenade Handrail
            GameObject railTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            railTop.name = "Promenade_RailTop";
            railTop.transform.SetParent(wallRoot.transform, false);
            railTop.transform.localPosition = new Vector3(0f, wallH + 0.55f, 0f);
            railTop.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            railTop.transform.localScale = new Vector3(0.08f, length * 0.5f, 0.08f);
            if (matMetal != null) railTop.GetComponent<Renderer>().sharedMaterial = matMetal;

            // Stanchions every 4m
            int postCount = Mathf.FloorToInt(length / 4f);
            for (int p = 0; p <= postCount; p++)
            {
                float pz = -length * 0.5f + p * 4f;
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"Rail_Post_{p}";
                post.transform.SetParent(wallRoot.transform, false);
                post.transform.localPosition = new Vector3(0f, wallH + 0.28f, pz);
                post.transform.localScale = new Vector3(0.06f, 0.28f, 0.06f);
                if (matMetal != null) post.GetComponent<Renderer>().sharedMaterial = matMetal;
            }

            return wallRoot;
        }

        public static GameObject CreateOceanWaterPlane(
            Transform parent,
            Vector3 position,
            Vector2 size,
            Material matWater)
        {
            GameObject waterObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            waterObj.name = "Ocean_Water_Plane";
            waterObj.transform.SetParent(parent, false);
            waterObj.transform.position = position;
            waterObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            waterObj.transform.localScale = new Vector3(size.x, size.y, 1f);
            Object.DestroyImmediate(waterObj.GetComponent<Collider>());
            if (matWater != null) waterObj.GetComponent<Renderer>().sharedMaterial = matWater;
            return waterObj;
        }

        // =================================================================================
        // 12. MARKET STALL & CANOPY BOOTH (MARKET DISTRICT)
        // =================================================================================

        public static GameObject CreateMarketStall(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matWood,
            Material matAwning,
            string stallName = "Market_Stall")
        {
            GameObject stallRoot = new GameObject($"Stall_{stallName}");
            stallRoot.transform.SetParent(parent, false);
            stallRoot.transform.position = position;
            stallRoot.transform.rotation = rotation;

            float w = 4.2f;
            float h = 2.8f;
            float d = 3.2f;

            // Wooden Counter Table
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Display_Table";
            table.transform.SetParent(stallRoot.transform, false);
            table.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            table.transform.localScale = new Vector3(w, 1.1f, d * 0.7f);
            if (matWood != null) table.GetComponent<Renderer>().sharedMaterial = matWood;

            // Striped Canvas Canopy Roof
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = "Canopy_Roof";
            canopy.transform.SetParent(stallRoot.transform, false);
            canopy.transform.localPosition = new Vector3(0f, h, 0f);
            canopy.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            canopy.transform.localScale = new Vector3(w + 0.6f, 0.08f, d + 0.6f);
            if (matAwning != null) canopy.GetComponent<Renderer>().sharedMaterial = matAwning;

            // Warm Hanging Lantern
            GameObject lantern = new GameObject("Stall_Lantern");
            lantern.transform.SetParent(stallRoot.transform, false);
            lantern.transform.localPosition = new Vector3(0f, h - 0.35f, 0f);
            Light l = lantern.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.78f, 0.40f);
            l.intensity = 8f;
            l.range = 7f;
            l.shadows = LightShadows.None;

            return stallRoot;
        }

        // =================================================================================
        // 13. SHIPPING CONTAINER & GANTRY CRANE (INDUSTRIAL PORT)
        // =================================================================================

        public static GameObject CreateShippingContainer(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matContainer,
            float length = 6.0f)
        {
            GameObject container = GameObject.CreatePrimitive(PrimitiveType.Cube);
            container.name = "Shipping_Container";
            container.transform.SetParent(parent, false);
            container.transform.position = position;
            container.transform.rotation = rotation;
            container.transform.localScale = new Vector3(2.44f, 2.59f, length);
            if (matContainer != null) container.GetComponent<Renderer>().sharedMaterial = matContainer;
            return container;
        }

        public static GameObject CreateCargoGantryCrane(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matSteel,
            Material matYellow)
        {
            GameObject craneRoot = new GameObject("Port_Gantry_Crane");
            craneRoot.transform.SetParent(parent, false);
            craneRoot.transform.position = position;
            craneRoot.transform.rotation = rotation;

            float span = 24f;
            float gantryH = 20f;

            // Left & Right Portal Towers
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * (span * 0.5f);
                GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tower.name = $"Portal_Leg_{side}";
                tower.transform.SetParent(craneRoot.transform, false);
                tower.transform.localPosition = new Vector3(x, gantryH * 0.5f, 0f);
                tower.transform.localScale = new Vector3(1.8f, gantryH, 2.2f);
                if (matSteel != null) tower.GetComponent<Renderer>().sharedMaterial = matSteel;
            }

            // Top Gantry Crossbeam (High-Visibility Safety Yellow)
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Gantry_Crossbeam";
            beam.transform.SetParent(craneRoot.transform, false);
            beam.transform.localPosition = new Vector3(0f, gantryH + 0.9f, 0f);
            beam.transform.localScale = new Vector3(span + 3f, 1.8f, 2.5f);
            if (matYellow != null) beam.GetComponent<Renderer>().sharedMaterial = matYellow;

            // Machinery Cab
            GameObject cab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cab.name = "Operator_Cab";
            cab.transform.SetParent(craneRoot.transform, false);
            cab.transform.localPosition = new Vector3(-4f, gantryH - 1.2f, 0f);
            cab.transform.localScale = new Vector3(3.2f, 2.2f, 2.8f);
            if (matSteel != null) cab.GetComponent<Renderer>().sharedMaterial = matSteel;

            // Downward High-Intensity Floodlight
            GameObject floodObj = new GameObject("Gantry_Floodlight");
            floodObj.transform.SetParent(craneRoot.transform, false);
            floodObj.transform.localPosition = new Vector3(0f, gantryH - 0.2f, 0f);
            floodObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Light fl = floodObj.AddComponent<Light>();
            fl.type = LightType.Spot;
            fl.color = new Color(0.95f, 0.98f, 1.0f);
            fl.intensity = 25f;
            fl.range = 35f;
            fl.spotAngle = 75f;
            fl.shadows = LightShadows.None;

            return craneRoot;
        }

        public static GameObject CreateFloodlightTower(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            Material matSteel,
            Material matBulb,
            float height = 15f)
        {
            GameObject mastRoot = new GameObject("Industrial_FloodlightTower");
            mastRoot.transform.SetParent(parent, false);
            mastRoot.transform.position = position;
            mastRoot.transform.rotation = rotation;

            // Tower Mast
            GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Tower_Mast";
            mast.transform.SetParent(mastRoot.transform, false);
            mast.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            mast.transform.localScale = new Vector3(0.45f, height * 0.5f, 0.45f);
            if (matSteel != null) mast.GetComponent<Renderer>().sharedMaterial = matSteel;

            // Crossbar
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "Light_Crossbar";
            bar.transform.SetParent(mastRoot.transform, false);
            bar.transform.localPosition = new Vector3(0f, height + 0.3f, 0f);
            bar.transform.localScale = new Vector3(3.8f, 0.3f, 0.4f);
            if (matSteel != null) bar.GetComponent<Renderer>().sharedMaterial = matSteel;

            // 4 Floodlights
            for (int i = 0; i < 4; i++)
            {
                float bx = -1.35f + i * 0.9f;
                GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lamp.name = $"Lamp_Head_{i}";
                lamp.transform.SetParent(bar.transform, false);
                lamp.transform.localPosition = new Vector3(bx, 0.35f, 0f);
                lamp.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
                lamp.transform.localScale = new Vector3(0.6f, 0.5f, 0.3f);
                if (matSteel != null) lamp.GetComponent<Renderer>().sharedMaterial = matSteel;

                GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Quad);
                bulb.name = "Bulb";
                bulb.transform.SetParent(lamp.transform, false);
                bulb.transform.localPosition = new Vector3(0f, 0f, -0.52f);
                bulb.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                bulb.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                Object.DestroyImmediate(bulb.GetComponent<Collider>());
                if (matBulb != null) bulb.GetComponent<Renderer>().sharedMaterial = matBulb;
            }

            // Downward Spotlight
            GameObject spotObj = new GameObject("Tower_Spotlight");
            spotObj.transform.SetParent(mastRoot.transform, false);
            spotObj.transform.localPosition = new Vector3(0f, height, 0f);
            spotObj.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            Light sp = spotObj.AddComponent<Light>();
            sp.type = LightType.Spot;
            sp.color = new Color(0.9f, 0.95f, 1f);
            sp.intensity = 30f;
            sp.range = 38f;
            sp.spotAngle = 80f;

            return mastRoot;
        }
    }
}
