using System;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Services.Transport;
using MCPForUnity.Editor.Services.Transport.Transports;

namespace MCPForUnity.Editor.Services
{
    /// <summary>
    /// Service locator for accessing MCP services without dependency injection
    /// </summary>
    public static class MCPServiceLocator
    {
        private static IBridgeControlService _bridgeService;
        private static IClientConfigurationService _clientService;
        private static IPathResolverService _pathService;
        private static ITestRunnerService _testRunnerService;
        private static IPackageUpdateService _packageUpdateService;
        private static IPlatformService _platformService;
        private static IToolDiscoveryService _toolDiscoveryService;
        private static IResourceDiscoveryService _resourceDiscoveryService;
        private static IServerManagementService _serverManagementService;
        private static TransportManager _transportManager;
        private static IPackageDeploymentService _packageDeploymentService;

        public static IBridgeControlService Bridge
        {
            get
            {
                if (_bridgeService == null) _bridgeService = new BridgeControlService();
                return _bridgeService;
            }
        }

        public static IClientConfigurationService Client
        {
            get
            {
                if (_clientService == null) _clientService = new ClientConfigurationService();
                return _clientService;
            }
        }

        public static IPathResolverService Paths
        {
            get
            {
                if (_pathService == null) _pathService = new PathResolverService();
                return _pathService;
            }
        }

        public static ITestRunnerService Tests
        {
            get
            {
                if (_testRunnerService == null) _testRunnerService = new TestRunnerService();
                return _testRunnerService;
            }
        }

        public static IPackageUpdateService Updates
        {
            get
            {
                if (_packageUpdateService == null) _packageUpdateService = new PackageUpdateService();
                return _packageUpdateService;
            }
        }

        public static IPlatformService Platform
        {
            get
            {
                if (_platformService == null) _platformService = new PlatformService();
                return _platformService;
            }
        }

        public static IToolDiscoveryService ToolDiscovery
        {
            get
            {
                if (_toolDiscoveryService == null) _toolDiscoveryService = new ToolDiscoveryService();
                return _toolDiscoveryService;
            }
        }

        public static IResourceDiscoveryService ResourceDiscovery
        {
            get
            {
                if (_resourceDiscoveryService == null) _resourceDiscoveryService = new ResourceDiscoveryService();
                return _resourceDiscoveryService;
            }
        }

        public static IServerManagementService Server
        {
            get
            {
                if (_serverManagementService == null) _serverManagementService = new ServerManagementService();
                return _serverManagementService;
            }
        }

        public static TransportManager TransportManager
        {
            get
            {
                if (_transportManager == null) _transportManager = new TransportManager();
                return _transportManager;
            }
        }

        public static IPackageDeploymentService Deployment
        {
            get
            {
                if (_packageDeploymentService == null) _packageDeploymentService = new PackageDeploymentService();
                return _packageDeploymentService;
            }
        }

        /// <summary>
        /// Registers a custom implementation for a service (useful for testing)
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="implementation">The implementation to register</param>
        public static void Register<T>(T implementation) where T : class
        {
            if (implementation is IBridgeControlService b)
                _bridgeService = b;
            else if (implementation is IClientConfigurationService c)
                _clientService = c;
            else if (implementation is IPathResolverService p)
                _pathService = p;
            else if (implementation is ITestRunnerService t)
                _testRunnerService = t;
            else if (implementation is IPackageUpdateService pu)
                _packageUpdateService = pu;
            else if (implementation is IPlatformService ps)
                _platformService = ps;
            else if (implementation is IToolDiscoveryService td)
                _toolDiscoveryService = td;
            else if (implementation is IResourceDiscoveryService rd)
                _resourceDiscoveryService = rd;
            else if (implementation is IServerManagementService sm)
                _serverManagementService = sm;
            else if (implementation is IPackageDeploymentService pd)
                _packageDeploymentService = pd;
            else if (implementation is TransportManager tm)
                _transportManager = tm;
        }

        /// <summary>
        /// Resets all services to their default implementations (useful for testing)
        /// </summary>
        public static void Reset()
        {
            (_bridgeService as IDisposable)?.Dispose();
            (_clientService as IDisposable)?.Dispose();
            (_pathService as IDisposable)?.Dispose();
            (_testRunnerService as IDisposable)?.Dispose();
            (_packageUpdateService as IDisposable)?.Dispose();
            (_platformService as IDisposable)?.Dispose();
            (_toolDiscoveryService as IDisposable)?.Dispose();
            (_resourceDiscoveryService as IDisposable)?.Dispose();
            (_serverManagementService as IDisposable)?.Dispose();
            (_transportManager as IDisposable)?.Dispose();
            (_packageDeploymentService as IDisposable)?.Dispose();

            _bridgeService = null;
            _clientService = null;
            _pathService = null;
            _testRunnerService = null;
            _packageUpdateService = null;
            _platformService = null;
            _toolDiscoveryService = null;
            _resourceDiscoveryService = null;
            _serverManagementService = null;
            _transportManager = null;
            _packageDeploymentService = null;
        }
    }
}
