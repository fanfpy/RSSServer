using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace UpdateFeed
{
    internal class TaskPool
    {
        private ConcurrentQueue<TaskInfo> taskQueue = new ConcurrentQueue<TaskInfo>();
        private int maxTaskNum;
        private int currentTaskNum;

        public TaskPool(int maxTaskNum)
        {
            this.maxTaskNum = maxTaskNum;
        }

        public void AddTask(TaskInfo task)
        {
            if (currentTaskNum >= maxTaskNum)
            {
                return;
            }

            taskQueue.Enqueue(task);
            task.Status = TaskStatus.Waiting;
            Interlocked.Increment(ref currentTaskNum);
            Task.Run(() => ExecuteTasks());
            PrintTaskStatus();
        }

        private void ExecuteTasks()
        {
            while (taskQueue.TryDequeue(out TaskInfo taskInfo))
            {
                taskInfo.Status = TaskStatus.Running;
                taskInfo.Task();
                taskInfo.Status = TaskStatus.Completed;
                Interlocked.Decrement(ref currentTaskNum);
                PrintTaskStatus();
            }
        }

        private void PrintTaskStatus()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Current task number: {currentTaskNum}, task queue length: {taskQueue.Count}\n\n");

            int idx = 1;
            foreach (var taskInfo in taskQueue)
            {
                string statusDescription = taskInfo.Status == TaskStatus.Waiting ? "waiting" :
                    (taskInfo.Status == TaskStatus.Running ? "running" : "completed");

                Console.WriteLine($"Task {idx++} ({taskInfo.Id}) is {statusDescription}.");
            }

            for (int i = currentTaskNum + 1; i <= maxTaskNum; i++)
            {
                Console.WriteLine($"Task {i} is pending.");
            }

            Console.SetCursorPosition(0, Console.CursorTop + 1);
        }
    }

    public class TaskInfo
    {
        public Action Task { get; set; }
        public string Id { get; set; }
        public TaskStatus Status { get; set; }
    }

    public enum TaskStatus
    {
        Waiting,
        Running,
        Completed
    }
}
