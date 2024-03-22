using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Elements;
using ExileCore.Shared;
using ExileCore.Shared.Helpers;

namespace AutoStash;

public class Stash
{
    public static bool IsVisible => StashElement is { IsVisible: true };

    public static List<string> TabNames =>
        GameController?.Game?.IngameState?.IngameUi?.StashElement?.AllStashNames?.ToList() ?? [];

    public static StashElement StashElement => GameController.IngameState.IngameUi.StashElement;

    private static GameController GameController => AutoStash.Instance.GameController;

    private static Element StashPanel => StashElement.ViewAllStashPanel;

    private static StashTopTabSwitcher TabSwitchBar => StashElement.StashTabContainer.TabSwitchBar;

    private static IList<Element> TabButtons => StashElement.TabListButtons;

    private static async SyncTask<bool> ScrollToTab(int stashIndex)
    {
        var isControlPressed = ExileCore.Input.IsKeyDown(Keys.LControlKey);

        await Input.MoveMouse(TabSwitchBar.GetClientRect().Center.ToVector2Num());
        await Input.Wait();

        if (!isControlPressed)
            await Input.KeyDown(Keys.LControlKey);

        while (StashElement.IndexVisibleStash != stashIndex)
        {
            if (stashIndex < StashElement.IndexVisibleStash)
                await Input.VerticalScroll(true, 1);
            else
                await Input.VerticalScroll(false, 1);

            await Input.Wait(10);
        }

        await TaskUtils.CheckEveryFrame(() => StashElement.AllInventories[stashIndex] != null,
            new CancellationTokenSource(2000).Token);

        if (!isControlPressed)
            await Input.KeyUp(Keys.LControlKey);

        return true;
    }

    private static bool IsTabVisible(int stashIndex)
    {
        if (!StashPanel.IsVisible) return false;

        var tabNode = TabButtons[stashIndex];

        return StashPanel.GetClientRect().Intersects(tabNode.GetClientRect());
    }

    public static async SyncTask<bool> ClickTab(int stashIndex)
    {
        if (!IsVisible)
            return false;

        if (StashElement.IndexVisibleStash == stashIndex)
            return true;

        if (IsTabVisible(stashIndex))
        {
            await Input.Click(TabButtons[stashIndex].GetClientRect().Center.ToVector2Num());
            await TaskUtils.CheckEveryFrame(() => StashElement.AllInventories[stashIndex] != null,
                new CancellationTokenSource(2000).Token);
        }
        else
        {
            return await ScrollToTab(stashIndex);
        }

        return true;
    }
}