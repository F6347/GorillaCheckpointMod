using BepInEx;
using Newtilla;
using UnityEngine;
using GorillaExtensions;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

namespace GorillaCheckpointMod
{
    [BepInDependency("Lofiat.Newtilla", "1.0.1")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        GameObject point;
        bool isPressingDown;
        Color VRRig_Color;

        void Start()
        {
            Newtilla.Newtilla.OnJoinModded += OnModdedJoined;
            Newtilla.Newtilla.OnLeaveModded += OnModdedLeft;
        }

        void OnEnable() => HarmonyPatches.ApplyHarmonyPatches();
        void OnDisable()
        {
            Destroy(point);
            HarmonyPatches.RemoveHarmonyPatches();
        }

        void Update()
        {
            if (!inRoom) return;

            if (Keyboard.current.yKey.isPressed || ControllerInputPoller.instance.leftControllerSecondaryButton)
            {
                if (!isPressingDown)
                    point.transform.position = GorillaLocomotion.Player.Instance.headCollider.transform.position;
            }
            else if (Keyboard.current.bKey.isPressed || ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                if (!isPressingDown)
                {
                    TeleportPlayer(point.transform.position);

                }
            }

            isPressingDown = Keyboard.current.yKey.isPressed || Keyboard.current.bKey.isPressed ||
                             ControllerInputPoller.instance.leftControllerSecondaryButton ||
                             ControllerInputPoller.instance.rightControllerSecondaryButton;
        }

        void OnModdedJoined(string modeName)
        {
            point = CreateCheckpoint();
            inRoom = true;
        }

        void OnModdedLeft(string modeName)
        {
            Destroy(point);
            inRoom = false;
        }

        GameObject CreateCheckpoint()
        {
            var checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player").GetComponent<VRRig>().playerColor
            };
            checkpoint.GetComponent<Renderer>().material = material;
            checkpoint.GetComponent<BoxCollider>().enabled = false;
            checkpoint.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
            return checkpoint;
        }

        void TeleportPlayer(Vector3 checkpointPos)
        {
            foreach (var collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                collider.enabled = false;
            GorillaLocomotion.Player.Instance.headCollider.transform.position = checkpointPos;
            Task.Run(async () =>
            {
                await Task.Delay(1);
                foreach (var collider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    collider.enabled = true;
            });
        }
    }
}