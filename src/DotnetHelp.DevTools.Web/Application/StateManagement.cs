using Blazored.LocalStorage;

namespace DotnetHelp.DevTools.Web.Application;

internal interface IStateManagement
{
	Task<string> GetIdAsync();
}

internal class StateManagement : IStateManagement
{
	private readonly ILocalStorageService _localStorage;

	public StateManagement(ILocalStorageService localStorage)
	{
		_localStorage = localStorage;
	}

	public async Task<string> GetIdAsync()
	{
		var id = await _localStorage.GetItemAsync<string>(LocalStorageKey.Id.ToString());
		if (id is null)
		{
			id = Guid.NewGuid().ToString("N");
			await _localStorage.SetItemAsync(LocalStorageKey.Id.ToString(), id);
		}
		return id;
	}

	internal enum LocalStorageKey
	{
		Id,
	}
}
