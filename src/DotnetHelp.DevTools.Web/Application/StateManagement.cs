using Blazored.LocalStorage;

namespace DotnetHelp.DevTools.Web.Application;

internal interface IStateManagement
{
	ValueTask<string> GetIdAsync(CancellationToken cancellationToken);
	ValueTask<OutgoingHttpRequest> GetOutgoingHttpRequestAsync(CancellationToken cancellationToken);
	ValueTask SetOutgoingHttpRequestAsync(OutgoingHttpRequest outgoingHttpRequest, CancellationToken cancellationToken);
}

internal class StateManagement : IStateManagement
{
	private readonly ILocalStorageService _localStorage;

	public StateManagement(ILocalStorageService localStorage)
	{
		_localStorage = localStorage;
	}

	public async ValueTask<string> GetIdAsync(CancellationToken cancellationToken)
	{
		var id = await _localStorage.GetItemAsync<string>(LocalStorageKey.Id.ToString(), cancellationToken);
		if (id is null)
		{
			id = Guid.NewGuid().ToString("N");
			await _localStorage.SetItemAsync(LocalStorageKey.Id.ToString(), id, cancellationToken);
		}
		return id;
	}

	public ValueTask<OutgoingHttpRequest> GetOutgoingHttpRequestAsync(CancellationToken cancellationToken)
	{
		return _localStorage.GetItemAsync<OutgoingHttpRequest>(LocalStorageKey.LastHttpRequest.ToString(), cancellationToken);
	}

	public ValueTask SetOutgoingHttpRequestAsync(OutgoingHttpRequest outgoingHttpRequest, CancellationToken cancellationToken)
	{
		return _localStorage.SetItemAsync(LocalStorageKey.LastHttpRequest.ToString(), outgoingHttpRequest, cancellationToken);
	}

	internal enum LocalStorageKey
	{
		Id,
		LastHttpRequest,
	}
}
