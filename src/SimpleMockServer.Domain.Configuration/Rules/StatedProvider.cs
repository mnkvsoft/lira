namespace SimpleMockServer.Domain.Configuration.Rules
{
    internal abstract class StatedProvider<TLoadEntity> : IStatedProvider
    {
        private readonly string _path;
        private ProviderState? _providerState;
        private readonly Func<string, Task<TLoadEntity>> _loadEntityFunc;
        protected Task<TLoadEntity> LoadTask;

        protected StatedProvider(IConfigurationPathProvider configuration, Func<string, Task<TLoadEntity>> loadEntityFunc)
        {
            _loadEntityFunc = loadEntityFunc;
            _path = configuration.Path;
            LoadTask = _loadEntityFunc(_path);
            configuration.Changed += OnChanged;
        }
    
        private void OnChanged(object? _, EventArgs __)
        {
            _providerState = null;
            LoadTask = _loadEntityFunc(_path);
        }
    
        public async Task<ProviderState> GetState()
        {
            if (_providerState != null)
                return _providerState;

            try
            {
                await LoadTask;
                _providerState = new ProviderState.Ok();
            }
            catch (Exception e)
            {
                _providerState = new ProviderState.Error(e);
            }

            return _providerState;
        }}
}
