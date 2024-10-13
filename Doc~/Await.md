# Await

## 扩展 Await

1. 获取主线程上下文 `SynchronizationContext`，`UnitySynchronizationContext.Post` 可以将代码运行在主线程上


```c#
class TestCustomAwaiter : MonoBehaviour
{
    public static SynchronizationContext UnitySynchronizationContext;
    void Start()
    {
        UnitySynchronizationContext = SynchronizationContext.Current;
    }
}
```

2. 定制异步 `Awater`，实现 `INotifyCompletion` 接口，包含三个必须的成员(`IsCompleted`，`GetResult`，`OnCompleted`)


```c#
class CustomAwaiter<T> : INotifyCompletion
{
    bool isCompleted;
    Exception exception;
    Action continuation;
    T result;
    
    public bool IsCompleted
    {
        get { return isCompleted; }
    }

    public T GetResult()
    {
        if (exception != null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        return result;
    }

    public void Complete(T result, Exception e)
    {
        isCompleted = true;
        exception = e;
        this.result = result;

        if (continuation != null)
        {
            TestCustomAwaiter.UnitySynchronizationContext.Post(o => continuation(), null);
        }
    }

    public void OnCompleted(Action continuation)
    {
        this.continuation = continuation;
    }

}
```



3. 为 `Func<T>` 类型扩展 `GetAwaiter` 方法，支持 `await` 关键字

```c#
public static CustomAwaiter<T> GetAwaiter<T>(this Func<T> action)
{
    var awaiter = new CustomAwaiter<T>();
    Task.Run(action)
        .ContinueWith(t =>
        {
            awaiter.Complete(t.Result, t.Exception);
        });
    return awaiter;
}
```

4. 测试运行，调用 `Run` 方法，`Run` 运行在主线程，`MyFunc` 运行在子线程


```c#
Func<string> MyFunc()
{
    return () =>
    {
        Debug.Log($"MyFunc Start ThreadId:" + Thread.CurrentThread.ManagedThreadId);
        int n = 3;
        while (n-- > 0)
        {
            Debug.Log($"MyFunc Running ThreadId:" + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);
        }
        Debug.Log($"MyFunc End " + Thread.CurrentThread.ManagedThreadId);
        return "Hello World";
    };
}

async Task Run()
{
    isRunning = true;
    Debug.Log($"Start " + Thread.CurrentThread.ManagedThreadId);
    Debug.Log($"threadId: {Thread.CurrentThread.ManagedThreadId}");

    string result = await MyFunc();
    Debug.Log("Result: " + result);
    Debug.Log("End " + Thread.CurrentThread.ManagedThreadId);
}
```

5. 运行结果

```tex
Start 1
MyFunc Start 2
MyFunc Running 2
MyFunc Running 2
MyFunc Running 2
MyFunc End 2
Result: Hello World
End 1
```

