using System.Collections.Generic;
using ExileCore2.Shared;

namespace AutoStash;

public class Scheduler
{
    private static readonly AutoStash Instance = AutoStash.Instance;
    public SyncTask<bool> CurrentTask;

    public Queue<SyncTask<bool>> Tasks = new();

    public Scheduler(params SyncTask<bool>[] tasks)
    {
        foreach (var task in tasks) Tasks.Enqueue(task);
    }

    public void AddTask(SyncTask<bool> task, string name = null)
    {
        if (name != null)
            Log.Debug($"Adding Task: {name}");
        else if (task != null) Log.Debug($"Adding Task: {task}");

        Tasks.Enqueue(task);
    }

    public void AddTasks(params SyncTask<bool>[] tasks)
    {
        foreach (var task in tasks) Tasks.Enqueue(task);
    }

    public void Run()
    {
        if (CurrentTask == null && Tasks.Count > 0)
        {
            CurrentTask = Tasks.Dequeue();
            CurrentTask.GetAwaiter().OnCompleted(() => { CurrentTask = null; });
        }

        if (CurrentTask != null)
        {
            Input.LockController = true;
            TaskUtils.RunOrRestart(ref CurrentTask, () => null);
        }
    }

    public void Stop()
    {
        CurrentTask = null;
    }

    public void Clear()
    {
        Tasks.Clear();
    }
}