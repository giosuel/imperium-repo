#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Console.Commands;
using Imperium.Core;
using Imperium.Core.Lifecycle;
using Imperium.Interface.ImperiumUI;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Console;

internal class ConsoleManager : ImpLifecycleObject
{
    private readonly Dictionary<string, ImpCommand> registeredLevelCommands = new();
    private readonly Dictionary<string, ImpCommand> registeredSpawnCommands = new();
    private readonly Dictionary<string, ImpCommand> registeredWindowCommands = new();
    private readonly Dictionary<string, ImpCommand> registeredActionCommands = new();
    private readonly Dictionary<string, ImpCommand> registeredSettingCommands = new();
    private readonly Dictionary<string, ImpCommand> registeredToggleCommands = new();

    private void Awake()
    {
        RegisterLevelCommands();
        Imperium.ObjectManager.CurrentLevelObjectsChanged += RegisterSpawnCommands;
    }

    internal void RegisterWindow<T>(string windowName) where T : ImperiumWindow
    {
        var wasAdded = registeredWindowCommands.TryAdd(NormalizeCommand(windowName), new ImpCommandWindow<T>(windowName));
        if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Duplicate window detected: {windowName}");
    }

    internal void RegisterAction(
        string actionName,
        Func<ConsoleQuery, bool> onExec,
        Func<ConsoleQuery, string> displayNameOverride = null,
        string interfacePath = null,
        Sprite customIcon = null,
        bool enabledBindingInverted = false,
        IBinding<bool> enabledBinding = null
    )
    {
        var wasAdded = registeredActionCommands.TryAdd(
            NormalizeCommand(actionName),
            new ImpCommandAction(
                actionName, onExec, displayNameOverride,
                interfacePath, customIcon,
                enabledBindingInverted, enabledBinding
            )
        );

        if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Action command with name {actionName} already exist.");
    }

    internal void RegisterSetting(
        string commandName,
        IBinding<bool> binding,
        string interfacePath = null,
        Sprite customIcon = null,
        bool enabledBindingInverted = false,
        IBinding<bool> enabledBinding = null
    )
    {
        var wasAdded = registeredToggleCommands.TryAdd(
            NormalizeCommand(commandName),
            new ImpCommandSettingToggle(
                commandName, binding, interfacePath, customIcon,
                enabledBindingInverted, enabledBinding
            )
        );

        if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Toggle command with name {commandName} already exist.");
    }

    internal void RegisterSetting(
        string commandName,
        IBinding<string> binding,
        string interfacePath = null,
        Sprite customIcon = null,
        bool enabledBindingInverted = false,
        IBinding<bool> enabledBinding = null
    )
    {
        var wasAdded = registeredSettingCommands.TryAdd(
            NormalizeCommand(commandName),
            new ImpCommandSettingString(
                commandName, binding, interfacePath, customIcon,
                enabledBindingInverted, enabledBinding
            )
        );

        if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Setting command with name {commandName} already exist.");
    }

    internal void RegisterSetting(
        string commandName,
        IBinding<int> binding,
        int? minValue = null,
        int? maxValue = null,
        string valueUnit = null,
        string interfacePath = null,
        Sprite customIcon = null,
        bool enabledBindingInverted = false,
        IBinding<bool> enabledBinding = null
    )
    {
        var wasAdded = registeredSettingCommands.TryAdd(
            NormalizeCommand(commandName),
            new ImpCommandSettingNumber(
                commandName, binding,
                minValue, maxValue, valueUnit,
                interfacePath, customIcon,
                enabledBindingInverted, enabledBinding
            )
        );

        if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Setting command with name {commandName} already exist.");
    }

    internal void RegisterSetting(
        string commandName,
        IBinding<float> binding,
        float? minValue = null,
        float? maxValue = null,
        string valueUnit = null,
        string interfacePath = null,
        Sprite customIcon = null,
        bool enabledBindingInverted = false,
        IBinding<bool> enabledBinding = null
    )
    {
        var wasAdded = registeredSettingCommands.TryAdd(
            NormalizeCommand(commandName),
            new ImpCommandSettingDecimal(
                commandName, binding,
                minValue, maxValue, valueUnit,
                interfacePath, customIcon,
                enabledBindingInverted, enabledBinding
            )
        );

        if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Setting command with name {commandName} already exist.");
    }

    /// <summary>
    ///     Tokenizes and parses a console query.
    /// </summary>
    /// <param name="rawQuery">The query string from the user input</param>
    internal static ConsoleQuery ParseQuery(string rawQuery)
    {
        var tokens = rawQuery.ToLower().Split(" ").Where(token => !string.IsNullOrEmpty(token)).ToArray();

        // How many tokens to skip before reading arguments
        var argumentSkip = 1;

        var query = tokens[0];
        var keywordIsQuery = false;
        QueryKeyword? keyword = null;

        /*
         * If the first argument is a keyword, we instead use the second token as the query.
         * If there is no second token, we re-use the keyword as the query.
         * We do this so users can search by keywords.
         */
        foreach (var queryKeyword in Enum.GetValues(typeof(QueryKeyword)).Cast<QueryKeyword>())
        {
            if (queryKeyword.ToString().ToLower().Contains(tokens[0]))
            {
                keyword = queryKeyword;
                argumentSkip = 2;
                if (tokens.Length > 1)
                {
                    query = tokens[1];
                }
                else
                {
                    query = tokens[0];
                    keywordIsQuery = true;
                }

                break;
            }
        }

#if DEBUG
        var argumentString = string.Join(", ", tokens.Skip(argumentSkip).ToArray());
        Imperium.IO.LogDebug(
            $"[CMD] Parsed Query {{ Query = {query}, Keyword = {keyword}, KeywordIsQuery = {keywordIsQuery}, Args = {argumentString} }}"
        );
#endif

        return new ConsoleQuery
        {
            Query = query,
            Keyword = keyword,
            KeywordIsQuery = keywordIsQuery,
            Args = tokens.Skip(argumentSkip).ToArray()
        };
    }

    /// <summary>
    ///     Searches through all registered commands with a provided query.
    /// </summary>
    /// <param name="query">The parsed console query from the user</param>
    internal ImpCommand[] Search(ConsoleQuery query)
    {
        var matchesLevel = IsLevelKeyword(query.Keyword);
        var matchesAction = IsActionKeyword(query.Keyword);
        var matchesWindow = IsWindowKeyword(query.Keyword);
        var matchesSpawn = IsSpawnKeyword(query.Keyword);
        var matchesSetting = IsSettingKeyword(query.Keyword);
        var matchesToggle = IsToggleKeyword(query.Keyword);

        return registeredActionCommands.Where(entry => IsIncluded(entry, matchesAction, query))
            .Concat(
                registeredSpawnCommands.Where(entry => IsIncluded(entry, matchesSpawn, query))
            )
            .Concat(
                registeredSettingCommands.Where(entry => IsIncluded(entry, matchesSetting, query))
            )
            .Concat(
                registeredToggleCommands.Where(entry => IsIncluded(entry, matchesToggle, query))
            )
            .Concat(
                registeredLevelCommands.Where(entry => IsIncluded(entry, matchesLevel, query))
            )
            .Concat(
                registeredWindowCommands
                    .Where(entry => IsIncluded(entry, matchesWindow, query))
            )
            .Select(entry => entry.Value)
            .ToArray();
    }

    private static bool IsIncluded(KeyValuePair<string, ImpCommand> entry, bool matchesKeyword, ConsoleQuery query)
    {
        if (!entry.Value.IsEnabled()) return false;

        if (query.Keyword == null) return entry.Key.Contains(query.Query);

        // Return all results that match the query or that are in the keyword's category
        if (query.KeywordIsQuery) return matchesKeyword || entry.Key.Contains(query.Query);

        // Return only results that match the query AND are in the keyword's category
        return matchesKeyword && entry.Key.Contains(query.Query);
    }

    private static bool IsActionKeyword(QueryKeyword? keyword)
    {
        return keyword is null;
    }

    private static bool IsLevelKeyword(QueryKeyword? keyword)
    {
        return keyword is QueryKeyword.Level or QueryKeyword.Load;
    }

    private static bool IsWindowKeyword(QueryKeyword? keyword)
    {
        return keyword is QueryKeyword.Open;
    }

    private static bool IsSettingKeyword(QueryKeyword? keyword)
    {
        return keyword is QueryKeyword.Set or QueryKeyword.Reset;
    }

    private static bool IsToggleKeyword(QueryKeyword? keyword)
    {
        return keyword is QueryKeyword.Enable or QueryKeyword.Disable or QueryKeyword.Reset or QueryKeyword.Toggle;
    }

    private static bool IsSpawnKeyword(QueryKeyword? keyword)
    {
        return keyword is QueryKeyword.Spawn;
    }

    private void RegisterLevelCommands()
    {
        foreach (var level in Imperium.ObjectManager.LoadedLevels.Value)
        {
            var wasAdded = registeredLevelCommands.TryAdd(
                NormalizeCommand(level.NarrativeName),
                new ImpCommandLevel(level)
            );
            if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Skipped duplicate level command: {level.NarrativeName}");
        }
    }

    private void RegisterSpawnCommands()
    {
        registeredSpawnCommands.Clear();

        foreach (var enemy in Imperium.ObjectManager.LoadedEntities.Value)
        {
            var wasAdded = registeredSpawnCommands.TryAdd(
                NormalizeCommand(enemy.EnemyName),
                new ImpCommandSpawn(enemy.EnemyName, enemy.EnemyName, SpawnObjectType.Entity)
            );
            if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Skipped duplicate enemy command: {enemy.EnemyName}");
        }

        foreach (var item in Imperium.ObjectManager.LoadedItems.Value)
        {
            var wasAdded = registeredSpawnCommands.TryAdd(
                NormalizeCommand(item.itemName),
                new ImpCommandSpawn(item.itemName, item.itemName, SpawnObjectType.Item)
            );
            if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Skipped duplicate item command: {item.itemName}");
        }

        foreach (var valuable in Imperium.ObjectManager.LoadedValuables.Value)
        {
            var wasAdded = registeredSpawnCommands.TryAdd(
                NormalizeCommand(valuable.name),
                new ImpCommandSpawn(valuable.name, valuable.name, SpawnObjectType.Valuable)
            );
            if (!wasAdded) Imperium.IO.LogWarning($"[CMD] Skipped duplicate valuable command: {valuable.name}");
        }
    }

    private static string NormalizeCommand(string command) => command.ToLower().Replace(" ", "");
}

internal readonly struct ConsoleQuery
{
    internal string Query { get; init; }
    internal string[] Args { get; init; }
    internal QueryKeyword? Keyword { get; init; }

    /*
     * Whether the keyword is used as part of the query. Set to true if only one token has been found.
     *
     * If set to true, all results that match the query or that are in the keyword's category are returned.
     * If set to false, exclusively results in the keyword's categories will be returned.
     */
    internal bool KeywordIsQuery { get; init; }
}

/// <summary>
///     Keywords are dynamic parts of the command's display name.
///     For example, toggle commands dynamically show "Enable XYZ" or "Disable XYZ" depending on their binding's current
///     state.
///     Keywords make it so the user can to query for these keywords "enable" and "disable" to get all commands of the type
///     ImpCommandSettingToggle without actually including their names in the query.
///     The keywords are matched via substring searching and are traversed from top to bottom. This means that the order of
///     this enum matters (e.g. why "Set" comes before "Reset").
/// </summary>
internal enum QueryKeyword
{
    /*
     * Keywords for ImpCommandSettingToggle commands.
     */
    Enable,
    Disable,
    Toggle,

    /*
     * Keywords for ImpCommandSpawn commands.
     */
    Spawn,

    /*
     * Keywords for ImpCommandWindow commands.
     */
    Open,

    /*
     * Keywords for ImpCommandSetting* commands.
     */
    Set,
    Reset,

    /*
     * Keywords for ImpCommandLevel commands.
     */
    Load,
    Level
}