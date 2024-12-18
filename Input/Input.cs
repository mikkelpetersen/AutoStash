using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms;
using ExileCore2.Shared;
using InputHumanizer.Input;

namespace AutoStash;

public class Input
{
    private static readonly AutoStash Instance = AutoStash.Instance;

    public static IInputController InputController;
    public static bool LockController = false;

    public static async SyncTask<bool> MoveMouse(Vector2 targetPosition)
    {
        Controller();
        await InputController.MoveMouse(
            GenerateRandomPosition(targetPosition + Instance.GameController.Window.GetWindowRectangle().TopLeft));
        EndController();
        return true;
    }

    public static async SyncTask<bool> Click(Vector2? targetPosition, MouseButtons mouseButton = MouseButtons.Left)
    {
        Controller();
        if (targetPosition != null)
            await InputController.MoveMouse(GenerateRandomPosition(targetPosition.Value));
        await InputController.Click(mouseButton);
        EndController();
        return true;
    }

    public new static async SyncTask<bool> Click(MouseButtons mouseButton = MouseButtons.Left)
    {
        return await Click(null, mouseButton);
    }

    public new static async SyncTask<bool> VerticalScroll(bool forward, int numberOfClicks)
    {
        Controller();
        await InputController.VerticalScroll(forward, numberOfClicks);
        EndController();

        return true;
    }

    public new static async SyncTask<bool> KeyDown(Keys key)
    {
        Controller();
        await InputController.KeyDown(key);
        EndController();

        return true;
    }

    public new static async SyncTask<bool> KeyUp(Keys key)
    {
        Controller();
        await InputController.KeyUp(key);
        EndController();

        return true;
    }

    public static async SyncTask<bool> Wait()
    {
        Controller();
        await Wait(InputController.GenerateDelay());
        EndController();

        return true;
    }

    public static async SyncTask<bool> Wait(int milliseconds)
    {
        return await Wait(TimeSpan.FromMilliseconds(milliseconds));
    }

    public static async SyncTask<bool> Wait(TimeSpan timePeriod)
    {
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < timePeriod) await TaskUtils.NextFrame();

        return true;
    }

    private static void Controller()
    {
        if (InputController != null)
            return;

        var tryGetInputController =
            Instance.GameController.PluginBridge.GetMethod<Func<string, IInputController>>(
                "InputHumanizer.TryGetInputController");

        if (tryGetInputController == null)
        {
            Log.Error("Unable to find InputHumanizer.");
            return;
        }

        InputController = tryGetInputController("AutoStash");

        if (InputController == null)
        {
            Log.Error("Unable to get InputHumanizer's InputController.");
            throw new Exception("Unable to get InputHumanizer's InputController.");
        }
    }

    private static void EndController()
    {
        if (LockController)
            return;

        InputController?.Dispose();
        InputController = null;
    }

    private static Vector2 GenerateRandomPosition(Vector2 targetPosition)
    {
        var xOffset = (float)(Random.Shared.NextDouble() * 10.0 - 5.0);
        var yOffset = (float)(Random.Shared.NextDouble() * 10.0 - 5.0);

        return new Vector2(targetPosition.X + xOffset, targetPosition.Y + yOffset);
    }
}