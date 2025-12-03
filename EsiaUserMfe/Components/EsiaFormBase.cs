using System;
using Microsoft.AspNetCore.Components;
using EsiaUserMfe.Dto;
using System.Net.Http.Json;
namespace EsiaUserMfe.Components;

public class EsiaFormBase : ComponentBase
{
    protected EsiaUserRequest Model { get; set; } = new();
    protected string? response;

    [Inject] public HttpClient Http { get; set; } = default!;

    protected async Task Submit()
    {
        response = "Отправка...";

        var resp = await Http.PostAsJsonAsync("/api/esia/create", Model);

        response = await resp.Content.ReadAsStringAsync();
    }
}
