using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MCPBridgeMod.Bridge;
using MCPBridgeMod.Contracts;

namespace MCPBridgeMod.Server;

public sealed class McpStdioServer
{
	private readonly BridgeCoordinator _coordinator;

	private readonly StdioRpcConnection _connection;

	public McpStdioServer(BridgeCoordinator coordinator)
	{
		_coordinator = coordinator;
		_connection = new StdioRpcConnection(Console.OpenStandardInput(), Console.OpenStandardOutput());
	}

	public async Task RunAsync(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			JsonDocument requestDocument = await _connection.ReadMessageAsync(cancellationToken);
			if (requestDocument == null)
			{
				break;
			}
			using (requestDocument)
			{
				await HandleMessageAsync(requestDocument.RootElement, cancellationToken);
			}
		}
	}

	private async Task HandleMessageAsync(JsonElement request, CancellationToken cancellationToken)
	{
		JsonElement methodElement;
		string method = (request.TryGetProperty("method", out methodElement) ? methodElement.GetString() : null);
		if (string.IsNullOrWhiteSpace(method))
		{
			return;
		}
		JsonElement idElement;
		bool hasId = request.TryGetProperty("id", out idElement);
		switch (method)
		{
		case "initialize":
			if (hasId)
			{
				JsonElement initializeParams;
				JsonElement protocolVersionElement;
				string requestedProtocolVersion = ((request.TryGetProperty("params", out initializeParams) && initializeParams.ValueKind == JsonValueKind.Object && initializeParams.TryGetProperty("protocolVersion", out protocolVersionElement) && protocolVersionElement.ValueKind == JsonValueKind.String) ? protocolVersionElement.GetString() : null);
				await _connection.WriteMessageAsync(new
				{
					jsonrpc = "2.0",
					id = CloneElement(idElement),
					result = new
					{
						protocolVersion = (requestedProtocolVersion ?? "2025-03-26"),
						capabilities = new
						{
							tools = new { }
						},
						serverInfo = new
						{
							name = "HeIsCommingAgent.MCPBridge",
							version = "0.1.0"
						}
					}
				}, cancellationToken);
			}
			break;
		case "notifications/initialized":
			break;
		case "tools/list":
			await ReplyAsync(idElement, CreateToolsListResult(), cancellationToken);
			break;
		case "tools/call":
			await HandleToolCallAsync(idElement, request, cancellationToken);
			break;
		case "ping":
			await ReplyAsync(idElement, new { }, cancellationToken);
			break;
		default:
			await WriteErrorAsync(idElement, -32601, "Method '" + method + "' is not supported.", cancellationToken);
			break;
		}
	}

	private async Task HandleToolCallAsync(JsonElement idElement, JsonElement request, CancellationToken cancellationToken)
	{
		if (!request.TryGetProperty("params", out var paramsElement) || !paramsElement.TryGetProperty("name", out var nameElement))
		{
			await WriteErrorAsync(idElement, -32602, "Tool name is required.", cancellationToken);
			return;
		}
		string toolName = nameElement.GetString();
		JsonElement argumentsElement;
		JsonElement arguments = (paramsElement.TryGetProperty("arguments", out argumentsElement) ? argumentsElement : default(JsonElement));
		switch (toolName)
		{
		case "bridge_get_handshake":
		{
			BridgeHandshake handshake = await _coordinator.GetHandshakeAsync(cancellationToken);
			await ReplyAsync(idElement, CreateToolResult(handshake, "Handshake ready for " + handshake.TargetGame + "."), cancellationToken);
			break;
		}
		case "bridge_get_snapshot":
		{
			GameSnapshot snapshot = await _coordinator.GetSnapshotAsync(cancellationToken);
			await ReplyAsync(idElement, CreateToolResult(snapshot, $"Screen={snapshot.Screen}; source={snapshot.Diagnostics.SourceMode}."), cancellationToken);
			break;
		}
		case "bridge_get_catalog":
		{
			GameCatalog catalog = await _coordinator.GetCatalogAsync(cancellationToken);
			await ReplyAsync(idElement, CreateToolResult(catalog, $"Items={catalog.Items.Count}; monsters={catalog.Monsters.Count}; maps={catalog.Maps.Count}; source={catalog.Diagnostics.SourceMode}."), cancellationToken);
			break;
		}
		case "bridge_list_actions":
		{
			IReadOnlyList<ActionDescriptor> actions = await _coordinator.ListActionsAsync(cancellationToken);
			await ReplyAsync(idElement, CreateToolResult(actions, $"Returned {actions.Count} available actions."), cancellationToken);
			break;
		}
		case "bridge_execute_action":
		{
			JsonElement actionElement;
			string actionId = ((arguments.ValueKind != JsonValueKind.Undefined && arguments.TryGetProperty("actionId", out actionElement)) ? actionElement.GetString() : null);
			if (string.IsNullOrWhiteSpace(actionId))
			{
				await WriteErrorAsync(idElement, -32602, "execute_action requires actionId.", cancellationToken);
				break;
			}
			IReadOnlyDictionary<string, string?> parameterMap = ReadParameterMap(arguments);
			ActionExecutionResult result = await _coordinator.ExecuteActionAsync(actionId, parameterMap, cancellationToken);
			await ReplyAsync(idElement, CreateToolResult(result, result.Message), cancellationToken);
			break;
		}
		default:
			await WriteErrorAsync(idElement, -32601, "Tool '" + toolName + "' is not registered.", cancellationToken);
			break;
		}
	}

	private static IReadOnlyDictionary<string, string?> ReadParameterMap(JsonElement argumentsElement)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (argumentsElement.ValueKind == JsonValueKind.Undefined || !argumentsElement.TryGetProperty("parameters", out var value) || value.ValueKind != JsonValueKind.Object)
		{
			return dictionary;
		}
		foreach (JsonProperty item in value.EnumerateObject())
		{
			Dictionary<string, string> dictionary2 = dictionary;
			string name = item.Name;
			JsonValueKind valueKind = item.Value.ValueKind;
			if (1 == 0)
			{
			}
			string value2 = valueKind switch
			{
				JsonValueKind.String => item.Value.GetString(), 
				JsonValueKind.Null => null, 
				_ => item.Value.ToString(), 
			};
			if (1 == 0)
			{
			}
			dictionary2[name] = value2;
		}
		return dictionary;
	}

	private static object CreateToolsListResult()
	{
		return new
		{
			tools = new object[5]
			{
				new
				{
					name = "bridge_get_handshake",
					description = "Return bridge identity, target game, loader, and exported MCP tools.",
					inputSchema = new
					{
						type = "object",
						properties = new { }
					}
				},
				new
				{
					name = "bridge_get_snapshot",
					description = "Return the latest game snapshot inferred from LocalLow save/settings/log files.",
					inputSchema = new
					{
						type = "object",
						properties = new { }
					}
				},
				new
				{
					name = "bridge_get_catalog",
					description = "Return the latest structured item, monster, map, and character data captured by the live plugin.",
					inputSchema = new
					{
						type = "object",
						properties = new { }
					}
				},
				new
				{
					name = "bridge_list_actions",
					description = "List the bridge action catalog that an agent can request.",
					inputSchema = new
					{
						type = "object",
						properties = new { }
					}
				},
				new
				{
					name = "bridge_execute_action",
					description = "Queue an action and optionally dispatch keyboard input to the game window.",
					inputSchema = new
					{
						type = "object",
						properties = new
						{
							actionId = new
							{
								type = "string",
								description = "Action identifier from bridge_list_actions."
							},
							parameters = new
							{
								type = "object",
								additionalProperties = true
							}
						},
						required = new string[1] { "actionId" }
					}
				}
			}
		};
	}

	private static object CreateToolResult<T>(T structuredContent, string text)
	{
		return new
		{
			content = new object[1]
			{
				new
				{
					type = "text",
					text = text
				}
			},
			structuredContent = structuredContent
		};
	}

	private async Task ReplyAsync(JsonElement idElement, object result, CancellationToken cancellationToken)
	{
		await _connection.WriteMessageAsync(new
		{
			jsonrpc = "2.0",
			id = CloneElement(idElement),
			result = result
		}, cancellationToken);
	}

	private async Task WriteErrorAsync(JsonElement idElement, int code, string message, CancellationToken cancellationToken)
	{
		await _connection.WriteMessageAsync(new
		{
			jsonrpc = "2.0",
			id = ((idElement.ValueKind == JsonValueKind.Undefined) ? null : CloneElement(idElement)),
			error = new { code, message }
		}, cancellationToken);
	}

	private static object? CloneElement(JsonElement element)
	{
		JsonValueKind valueKind = element.ValueKind;
		if (1 == 0)
		{
		}
		long value;
		object result = valueKind switch
		{
			JsonValueKind.String => element.GetString(), 
			JsonValueKind.Number => (!element.TryGetInt64(out value)) ? ((object)element.GetDouble()) : ((object)value), 
			JsonValueKind.True => true, 
			JsonValueKind.False => false, 
			_ => JsonSerializer.Deserialize<object>(element.GetRawText()), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
