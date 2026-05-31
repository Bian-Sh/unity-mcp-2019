using System.Collections.Generic;

namespace MCPForUnity.Editor.Models
{
    public class McpClient
    {
        public string name;
        public string windowsConfigPath;
        public string macConfigPath;
        public string linuxConfigPath;
        public string configStatus;
        public McpStatus status = McpStatus.NotConfigured;
        public ConfiguredTransport configuredTransport = ConfiguredTransport.Unknown;

        // Capability flags/config for JSON-based configurators
        public bool IsVsCodeLayout; // Whether the config file follows VS Code layout (env object at root)
        public bool SupportsHttpTransport = true; // Whether the MCP server supports HTTP transport
        public bool EnsureEnvObject; // Whether to ensure the env object is present in the config
        public bool StripEnvWhenNotRequired; // Whether to strip the env object when not required
        public string HttpUrlProperty = "url"; // The property name for the HTTP URL in the config
        public Dictionary<string, object> DefaultUnityFields = new Dictionary<string, object>();

        // Helper method to convert the enum to a display string
        public string GetStatusDisplayString()
        {
            switch (status)
            {
                case McpStatus.NotConfigured: return "Not Configured";
                case McpStatus.Configured: return "Configured";
                case McpStatus.Running: return "Running";
                case McpStatus.Connected: return "Connected";
                case McpStatus.IncorrectPath: return "Incorrect Path";
                case McpStatus.CommunicationError: return "Communication Error";
                case McpStatus.NoResponse: return "No Response";
                case McpStatus.UnsupportedOS: return "Unsupported OS";
                case McpStatus.MissingConfig: return "Missing MCPForUnity Config";
                case McpStatus.Error: return configStatus != null && configStatus.StartsWith("Error:") ? configStatus : "Error";
                case McpStatus.VersionMismatch: return "Version Mismatch";
                default: return "Unknown";
            }
        }

        // Helper method to set both status enum and string for backward compatibility
        public void SetStatus(McpStatus newStatus, string errorDetails = null)
        {
            status = newStatus;

            if ((newStatus == McpStatus.Error || newStatus == McpStatus.VersionMismatch) && !string.IsNullOrEmpty(errorDetails))
            {
                configStatus = errorDetails;
            }
            else
            {
                configStatus = GetStatusDisplayString();
            }
        }
    }
}
