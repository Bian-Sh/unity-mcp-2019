using System;
using MCPForUnity.Editor.Helpers;
using Newtonsoft.Json.Linq;
#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;

namespace MCPForUnity.Editor.Resources.Editor
{
    /// <summary>
    /// Returns information about the currently open Prefab Stage (Isolation or
    /// In-Context mode), or { isOpen = false } when no prefab is being edited.
    ///
    /// Wires up the existing <c>mcpforunity://editor/prefab-stage</c> resource
    /// (Server/src/services/resources/prefab_stage.py) which dispatches the
    /// <c>get_prefab_stage</c> command name expected by this handler.
    /// </summary>
    [McpForUnityResource("get_prefab_stage")]
    public static class GetPrefabStage
    {
        public static object HandleCommand(JObject @params)
        {
            try
            {
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage == null)
                    return new SuccessResponse("No prefab stage open.", new { isOpen = false });

                var root = stage.prefabContentsRoot;
                return new SuccessResponse("Retrieved prefab stage info.", new
                {
                    isOpen = true,
                    assetPath = GetPrefabStageAssetPath(stage),
                    prefabRootName = root != null ? root.name : null,
#if UNITY_2020_1_OR_NEWER
                    mode = stage.mode.ToString(),
#else
                    mode = "Unknown",
#endif
                    isDirty = stage.scene.isDirty,
                });
            }
            catch (Exception e)
            {
                return new ErrorResponse($"Error getting prefab stage: {e.Message}");
            }
        }

        private static string GetPrefabStageAssetPath(PrefabStage stage)
        {
#if UNITY_2020_1_OR_NEWER
            return stage.assetPath;
#else
            return stage.prefabAssetPath;
#endif
        }
    }
}
