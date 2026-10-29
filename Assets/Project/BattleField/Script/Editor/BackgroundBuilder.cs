using System.IO;
using BattleField.Script.Background;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace BattleField.Script.Editor
{
    // BattleField シーンに「溶岩島」「森の中」「データ空間」の背景を組み立て、
    // 今の草原の地形とあわせて、試合ごとにランダムで 1 つ選ばれるようにする。
    // 取り込んだ素材(Stylized Lava Materials / RekindledFX / Hand Painted Nature Kit / SDF Training Stage)を使う。
    // メニュー「Tools > Cardgame > 背景を組み立てる」から実行する。何度実行しても、作り直されるだけで増えない。
    public static class BackgroundBuilder
    {
        private const string ScenePath = "Assets/Project/BattleField/Scene/BattleField.unity";
        private const string OutDir = "Assets/Project/BattleField/Backgrounds";

        private const string LavaDir = "Assets/Stylized Lava Materials/Lava01";
        private const string LavaPoolPrefab = "Assets/RekindledFX/Elemental/Prefabs/LavaPool.prefab";
        private const string NatureDir = "Assets/Silver_Cats/Hand_Painted_Nature_Kit_LITE";
        private const string ForestGroundLayer = NatureDir + "/Terrain_Layers/Layer_Forest_Ground.terrainlayer";
        private const string ForestSkybox = NatureDir + "/Skyboxes/Sky_01_Sun_Cloud.mat";
        private const string GridTexture = "Assets/SDF_BasicTrainingStage/Textures/Enviornmants/Training_Stage/Grid_04.png";

        private const string RootName = "Backgrounds";
        private const string GrassName = "Background_Grass";

        // カメラ(45.6, 3.0, 53.1)が奥(+Z)を向き、カードが出る場所は (45.6, ?, 64.3) 付近
        private static readonly Vector2 FallbackCenter = new Vector2(45.6f, 64.3f);

        [MenuItem("Tools/Cardgame/背景を組み立てる")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            EnsureFolder(OutDir);

            // 以前の版で作った、いまは使わないマテリアル(ボール状の岩用)
            AssetDatabase.DeleteAsset($"{OutDir}/Lava_Boulder.mat");

            var center = FindCenter(scene);
            var terrain = Object.FindObjectOfType<Terrain>(true);
            float ground = terrain != null ? terrain.SampleHeight(new Vector3(center.x, 0f, center.y)) + terrain.transform.position.y : 0f;
            var origin = new Vector3(center.x, ground, center.y);
            Debug.Log($"背景の中心: {origin}(地面の高さ {ground})");

            // 作り直しのため、前回の「溶岩島」「森の中」「データ空間」を消す
            var root = GameObject.Find(RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
            }
            for (int i = root.transform.childCount - 1; i >= 0; i--)
            {
                var child = root.transform.GetChild(i).gameObject;
                if (child.name != GrassName) Object.DestroyImmediate(child);
            }

            var grass = root.transform.Find(GrassName)?.gameObject;
            if (grass == null)
            {
                grass = new GameObject(GrassName);
                grass.transform.SetParent(root.transform, false);
            }
            if (terrain != null && terrain.transform.parent != grass.transform)
            {
                terrain.transform.SetParent(grass.transform, true);
            }

            var lava = BuildLava(root.transform, origin);
            var forest = BuildForest(root.transform, origin);
            var data = BuildData(root.transform, origin);

            grass.SetActive(true);
            lava.SetActive(false);
            forest.SetActive(false);
            data.SetActive(false);

            SetupSelector(grass, lava, forest, data);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("背景を組み立てました(草原・溶岩島・森の中・データ空間)。BackgroundSelector の Forced Index で、1 つに固定して確認できます");
        }

        // ---------- 溶岩島 ----------

        private static GameObject BuildLava(Transform parent, Vector3 origin)
        {
            var root = NewRoot("Background_Lava", parent);

            var rockBase = Load<Texture2D>(LavaDir + "/T_Lava01_basecolor.png");
            var rockEmission = Load<Texture2D>(LavaDir + "/T_Lava01_emissive.png");
            var island = MakeLit("Lava_IslandRock", rockBase, rockEmission, new Color(0.55f, 0.5f, 0.55f), new Color(1.1f, 0.35f, 0.08f), new Vector2(5f, 5f));

            // 溶岩の海
            var poolPrefab = Load<GameObject>(LavaPoolPrefab);
            if (poolPrefab != null)
            {
                var pool = (GameObject)PrefabUtility.InstantiatePrefab(poolPrefab, root.transform);
                pool.name = "LavaSea";
                pool.transform.position = new Vector3(origin.x, origin.y - 0.45f, origin.z);

                // 元の設定だと、面が大きく波打って島より高くなり、カードが見えにくい。波と流れを小さくする
                var seaRenderer = pool.GetComponent<MeshRenderer>();
                if (seaRenderer != null) seaRenderer.sharedMaterial = MakeCalmLava(seaRenderer.sharedMaterial);

                CalmEmbers(pool);
            }
            else
            {
                Debug.LogWarning($"LavaPool が見つかりません: {LavaPoolPrefab}");
            }

            // カードが出る島
            var disc = NewDisc("Island", root.transform, new Vector3(origin.x, origin.y - 0.5f, origin.z), 26f, 1f, island);
            disc.isStatic = true;

            // 溶岩のあかり
            var lightObject = new GameObject("LavaGlow");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.position = origin + new Vector3(0f, 3f, 4f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.45f, 0.15f);
            light.range = 45f;
            light.intensity = 8f;

            return root;
        }

        // 溶岩の海のマテリアルを複製して、波・流れ・光の強さを控えめにする(元のファイルは変えない)
        private static Material MakeCalmLava(Material source)
        {
            if (source == null) return null;

            var path = $"{OutDir}/Lava_Sea_Calm.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(source);
                AssetDatabase.CreateAsset(mat, path);
            }

            // 海は 15 倍に拡大されているので、波の高さはとても小さい値にする
            SetFloat(mat, "_DisplacementAmp", 0.004f);
            SetFloat(mat, "_DisplacementSpeed", 0.4f);
            SetFloat(mat, "_FlowSpeed", 0.12f);
            SetFloat(mat, "_Turbulence", 0.5f);
            SetFloat(mat, "_BubbleIntensity", 0.15f);
            SetFloat(mat, "_BubbleSpeed", 0.3f);
            SetFloat(mat, "_EmissionIntensity", 2.5f);
            SetFloat(mat, "_GlowPulseAmount", 0.04f);
            SetFloat(mat, "_GlowPulseSpeed", 0.8f);
            SetFloat(mat, "_HotWhiteBoost", 0.1f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void SetFloat(Material mat, string property, float value)
        {
            if (mat.HasProperty(property)) mat.SetFloat(property, value);
            else Debug.LogWarning($"溶岩のマテリアルに {property} がありません");
        }

        // 火の粉は、海と同じ 15 倍に拡大されて、画面いっぱいのぼやけた玉になっていた。大きさを元に戻し、数も減らす
        private static void CalmEmbers(GameObject pool)
        {
            var embers = pool.GetComponentInChildren<RekindledFX.Elemental.LavaEmbers>(true);
            if (embers == null) return;

            var particles = embers.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                main.scalingMode = ParticleSystemScalingMode.Local;
            }

            embers.area = new Vector2(46f, 46f);
            embers.rate = 40f;
            embers.size = new Vector2(0.03f, 0.07f);
            embers.riseSpeed = 0.5f;
            EditorUtility.SetDirty(embers);
        }

        // ---------- 森の中 ----------

        private static GameObject BuildForest(Transform parent, Vector3 origin)
        {
            var root = NewRoot("Background_Forest", parent);

            var layer = Load<TerrainLayer>(ForestGroundLayer);
            var groundMat = MakeLit("Forest_Ground", layer != null ? layer.diffuseTexture : null, null, new Color(0.8f, 0.85f, 0.75f), Color.black, new Vector2(24f, 24f));
            NewDisc("ForestGround", root.transform, new Vector3(origin.x, origin.y - 0.2f, origin.z), 150f, 0.4f, groundMat);

            var rng = new Random(2024);
            PlaceMany(root.transform, rng, origin, new[] { "Pine_Tree", "Cedar_Tree_03", "Larch_Tree" }, 70, 11f, 60f, 9f, 15f);
            // モンスターが立つ場所(中心から左右に約 5)に重ならないよう、小さな植物は少し離して置く
            PlaceMany(root.transform, rng, origin, new[] { "Fern", "Plantain", "Dandelion_02" }, 40, 8f, 26f, 0.6f, 1.1f);
            PlaceMany(root.transform, rng, origin, new[] { "Stump" }, 6, 9f, 24f, 0.8f, 1.2f);
            PlaceMany(root.transform, rng, origin, new[] { "Boletus_Edulis" }, 10, 8f, 22f, 0.3f, 0.5f);

            return root;
        }

        private static void PlaceMany(Transform parent, Random rng, Vector3 origin, string[] prefabNames, int count, float minRadius, float maxRadius, float minHeight, float maxHeight)
        {
            for (int i = 0; i < count; i++)
            {
                var prefab = Load<GameObject>($"{NatureDir}/Prefabs/{prefabNames[rng.Next(prefabNames.Length)]}.prefab");
                if (prefab == null) continue;

                var p = RingPoint(rng, origin, minRadius, maxRadius, true);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                go.transform.position = new Vector3(p.x, origin.y, p.z);
                go.transform.rotation = Quaternion.Euler(0f, Range(rng, 0f, 360f), 0f);

                // 素材の元の大きさに左右されないよう、高さを目標に合わせて拡大縮小し、根元を地面に置く
                FitHeight(go, Range(rng, minHeight, maxHeight));
                SitOnGround(go, origin.y);
            }
        }

        // ---------- データ空間 ----------

        private static GameObject BuildData(Transform parent, Vector3 origin)
        {
            var root = NewRoot("Background_Data", parent);

            var grid = Load<Texture2D>(GridTexture);
            var floorMat = MakeLit("Data_Floor", grid, grid, new Color(0.05f, 0.08f, 0.12f), new Color(0f, 1.4f, 1.8f), new Vector2(6f, 6f));
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "GridFloor";
            floor.transform.SetParent(root.transform, false);
            RemoveCollider(floor);
            floor.transform.position = new Vector3(origin.x, origin.y, origin.z);
            floor.transform.localScale = new Vector3(15f, 1f, 15f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            var cyan = MakeLit("Data_PillarCyan", null, null, new Color(0.02f, 0.05f, 0.08f), new Color(0f, 2.2f, 2.6f), Vector2.one);
            var magenta = MakeLit("Data_PillarMagenta", null, null, new Color(0.05f, 0.02f, 0.08f), new Color(2.4f, 0.2f, 2.2f), Vector2.one);

            var rng = new Random(7);

            // 周りにそびえる光の柱
            for (int i = 0; i < 28; i++)
            {
                var p = RingPoint(rng, origin, 14f, 60f, true);
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = "Pillar" + (i + 1);
                pillar.transform.SetParent(root.transform, false);
                RemoveCollider(pillar);
                float height = Range(rng, 6f, 38f);
                float width = Range(rng, 0.4f, 1.4f);
                pillar.transform.position = new Vector3(p.x, origin.y + height * 0.5f, p.z);
                pillar.transform.localScale = new Vector3(width, height, width);
                pillar.GetComponent<MeshRenderer>().sharedMaterial = rng.Next(3) == 0 ? magenta : cyan;
            }

            // 宙に浮かぶデータのかけら
            for (int i = 0; i < 45; i++)
            {
                var p = RingPoint(rng, origin, 9f, 55f, true);
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Fragment" + (i + 1);
                cube.transform.SetParent(root.transform, false);
                RemoveCollider(cube);
                float size = Range(rng, 0.3f, 1.2f);
                cube.transform.position = new Vector3(p.x, origin.y + Range(rng, 1f, 22f), p.z);
                cube.transform.localScale = Vector3.one * size;
                cube.transform.rotation = Quaternion.Euler(Range(rng, 0f, 90f), Range(rng, 0f, 90f), Range(rng, 0f, 90f));
                cube.GetComponent<MeshRenderer>().sharedMaterial = rng.Next(4) == 0 ? magenta : cyan;
            }

            return root;
        }

        // ---------- 選択部品 ----------

        private static void SetupSelector(GameObject grass, GameObject lava, GameObject forest, GameObject data)
        {
            var selectorObject = GameObject.Find("BackgroundSelector");
            if (selectorObject == null) selectorObject = new GameObject("BackgroundSelector");
            var selector = selectorObject.GetComponent<BackgroundSelector>();
            if (selector == null) selector = selectorObject.AddComponent<BackgroundSelector>();

            var lightObject = GameObject.Find("Directional Light");
            var mainLight = lightObject != null ? lightObject.GetComponent<Light>() : null;

            var lavaSky = MakeSkybox("Sky_Lava", new Color(0.55f, 0.16f, 0.06f), new Color(0.1f, 0.03f, 0.02f), 0.9f, 1.5f);
            var dataSky = MakeSkybox("Sky_Data", new Color(0.02f, 0.1f, 0.18f), new Color(0f, 0.03f, 0.06f), 0.7f, 1.2f);
            var forestSky = Load<Material>(ForestSkybox);
            if (forestSky == null) Debug.LogWarning($"森の空が見つかりません: {ForestSkybox}");

            var so = new SerializedObject(selector);
            so.FindProperty("mainLight").objectReferenceValue = mainLight;
            so.FindProperty("forcedIndex").intValue = -1;

            var list = so.FindProperty("backgrounds");
            list.arraySize = 4;
            SetEntry(list.GetArrayElementAtIndex(0), "草原", grass, null, false, default, 0f, false, default, 1f);
            SetEntry(list.GetArrayElementAtIndex(1), "溶岩島", lava, lavaSky, true, new Color(0.32f, 0.1f, 0.04f), 0.014f, true, new Color(1f, 0.55f, 0.3f), 0.8f);
            SetEntry(list.GetArrayElementAtIndex(2), "森の中", forest, forestSky, true, new Color(0.45f, 0.55f, 0.42f), 0.016f, true, new Color(1f, 0.93f, 0.75f), 1.1f);
            SetEntry(list.GetArrayElementAtIndex(3), "データ空間", data, dataSky, true, new Color(0f, 0.07f, 0.12f), 0.012f, true, new Color(0.6f, 0.9f, 1f), 0.9f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEntry(SerializedProperty entry, string name, GameObject root, Material skybox,
            bool fog, Color fogColor, float fogDensity, bool overrideLight, Color lightColor, float lightIntensity)
        {
            entry.FindPropertyRelative("name").stringValue = name;
            entry.FindPropertyRelative("root").objectReferenceValue = root;
            entry.FindPropertyRelative("skybox").objectReferenceValue = skybox;
            entry.FindPropertyRelative("useFog").boolValue = fog;
            entry.FindPropertyRelative("fogColor").colorValue = fogColor;
            entry.FindPropertyRelative("fogDensity").floatValue = fogDensity;
            entry.FindPropertyRelative("overrideLight").boolValue = overrideLight;
            entry.FindPropertyRelative("lightColor").colorValue = lightColor;
            entry.FindPropertyRelative("lightIntensity").floatValue = lightIntensity;
        }

        // ---------- 部品 ----------

        private static Vector2 FindCenter(Scene scene)
        {
            Vector3? player = null, enemy = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "PlEffect") player = root.transform.position;
                if (root.name == "EnEffect") enemy = root.transform.position;
            }
            if (player.HasValue && enemy.HasValue)
            {
                return new Vector2((player.Value.x + enemy.Value.x) * 0.5f, (player.Value.z + enemy.Value.z) * 0.5f);
            }
            return FallbackCenter;
        }

        // 遊ぶ場所のまわりの輪の中から、1 点を選ぶ。カメラから遊ぶ場所までの視界はあけておく
        private static Vector3 RingPoint(Random rng, Vector3 origin, float minRadius, float maxRadius, bool keepViewClear)
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                float angle = Range(rng, 0f, Mathf.PI * 2f);
                float radius = Mathf.Lerp(minRadius, maxRadius, Mathf.Sqrt((float)rng.NextDouble()));
                float dx = Mathf.Cos(angle) * radius;
                float dz = Mathf.Sin(angle) * radius;

                if (keepViewClear)
                {
                    bool inViewCorridor = Mathf.Abs(dx) < 6f && dz < 2f;   // カメラから中央までの道
                    bool behindCamera = dz < -13f;                          // カメラの後ろは見えない
                    if (inViewCorridor || behindCamera) continue;
                }
                return new Vector3(origin.x + dx, origin.y, origin.z + dz);
            }
            return new Vector3(origin.x + maxRadius, origin.y, origin.z + maxRadius);
        }

        private static float Range(Random rng, float min, float max) => min + (float)rng.NextDouble() * (max - min);

        private static GameObject NewRoot(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        // 平たい円柱を作る。上面の高さは「center.y + 厚みの半分」なので、
        // 地面の高さにそろえたいときは、呼び出し側で center.y を「地面 - 厚みの半分」にして渡す
        private static GameObject NewDisc(string name, Transform parent, Vector3 center, float diameter, float thickness, Material material)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            disc.transform.SetParent(parent, false);
            RemoveCollider(disc);
            disc.transform.position = center;
            // 円柱は高さ 2 が標準なので、厚みの半分をスケールに入れる
            disc.transform.localScale = new Vector3(diameter, thickness * 0.5f, diameter);
            disc.GetComponent<MeshRenderer>().sharedMaterial = material;
            return disc;
        }

        private static void RemoveCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }

        private static void FitHeight(GameObject go, float targetHeight)
        {
            var bounds = GetBounds(go);
            if (bounds.size.y <= 0.0001f) return;
            go.transform.localScale *= targetHeight / bounds.size.y;
        }

        private static void SitOnGround(GameObject go, float groundY)
        {
            var bounds = GetBounds(go);
            go.transform.position += new Vector3(0f, groundY - bounds.min.y, 0f);
        }

        private static Bounds GetBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        // ---------- マテリアル ----------

        private static Material MakeLit(string name, Texture baseMap, Texture emissionMap, Color baseColor, Color emissionColor, Vector2 tiling)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var path = $"{OutDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.shader = shader;

            mat.SetColor("_BaseColor", baseColor);
            mat.SetTexture("_BaseMap", baseMap);
            mat.SetTextureScale("_BaseMap", tiling);
            mat.SetFloat("_Smoothness", 0.15f);
            mat.SetFloat("_Metallic", 0f);

            bool emissive = emissionColor.maxColorComponent > 0.001f;
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
                mat.SetTexture("_EmissionMap", emissionMap);
                mat.SetTextureScale("_EmissionMap", tiling);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material MakeSkybox(string name, Color skyTint, Color groundColor, float exposure, float atmosphere)
        {
            var path = $"{OutDir}/{name}.mat";
            var shader = Shader.Find("Skybox/Procedural");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.shader = shader;
            mat.SetColor("_SkyTint", skyTint);
            mat.SetColor("_GroundColor", groundColor);
            mat.SetFloat("_Exposure", exposure);
            mat.SetFloat("_AtmosphereThickness", atmosphere);
            mat.SetFloat("_SunSize", 0.02f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.LogWarning($"素材が見つかりません: {path}");
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
