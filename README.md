# Async

支持 Unity 使用 `await/async` 异步关键字



| 特性                   | Async | Async (Editor) | Coroutine |
| ---------------------- | ----- | -------------- | --------- |
| IEnumerator            | ✔     | ✔              | ✔         |
| YieldInstruction       | ✔     | ✘              | ✔         |
| AsyncOperation         | ✔     | ✔              | ✔         |
| CustomYieldInstruction | ✔     | ✔              | ✔         |
| 主/子线程切换          | ✔     | ✔              |           |



## 异步与协程转换

### 协程转异步

协程方法调用 `Await` 转为 `Task`，支持 `await` 异步等待

```c#
IEnumerator Routine_WaitForSeconds(float seconds)
{
    yield return new WaitForSeconds(seconds);
}

async Task Coroutine_To_Async()
{
    await Routine_WaitForSeconds(0.1f).Await();
    //Main Thread, 0.1s after ...
}

//run
Coroutine_To_Async();
```



### 异步转协程

`Task` 使用`AsRoutine` 转为 `IEnumerator` 协程可等待对象 

```c#
async Task Async_WaitForSeconds(float seconds)
{
    await new WaitForSeconds(seconds);
}

IEnumerator Async_To_Coroutine()
{
    yield return Async_WaitForSeconds(0.1f).AsRoutine();
    //Main Thread, 0.1s after ...
}

//run
StartCoroutine(Async_To_Coroutine());
```



## 线程切换

线程切换可以决定在主线程或非主线程运行，区分主线程和子线程通过比对当前线程Id和主线程Id是否相等

**线程类型**

- MainThread

  主线程，运行时为 `MonoBehaviour` 协程

- EditorMainThread

  编辑器主线程，非运行时模拟协程

- SubThread

  子线程, `Task.Run`

### 切换到主线程

```c#
//Sub Thread
await new MainThread();
//Main Thread
```

任务最终调用 Unity `StartCoroutine` 以协程运行

### **切换到子线程**

不阻塞主线程

```c#
//Main Thread
await new SubThread();
//Sub Thread
```

### 线程域

延续任务继续在之前的线程上运行

**主线程**

```c#
await new MainThread();
//主线程
await new WaitForEndOfFrame(); //运行在主线程
//继续主线程
```

**执行延续任务**

- 如果当前为主线程，则立即执行

- 如果当前为子线程，则获取当前主线程调用 `Post` 运行延续任务

**子线程**

```c#
await new SubThread();
//子线程
await new WaitForEndOfFrame(); //运行在主线程
//切换回子线程
```

`await new WaitForEndOfFrame()` 运行在主线程，等待完成后在子线程执行延续任务

**执行延续任务**

- 如果当前为子线程，则立即执行

- 如果当前为主线程，则 `Task.Run` 开启一个新的子线程执行延续任务



###  等待子线程

```c#
//Main Thread
Task[] tasks = new Task[10];
for (int i = 0; i < tasks.Length; i++)
{
    tasks[i] = Task.Delay(100);
}

//切换到子线程等待所有任务完成，不阻塞主线程
await new SubThread();
//Sub Thread
Task.WaitAll(tasks);

await new MainThread();
//Main Thread, 100ms after...
```
所有 `tasks` 都在子线程运行，`await WaitFor.SubThread` 在子线程等待所有 `tasks` 任务完成，没有阻塞主线程

## 异步代码

当等待 Unity 对象为 `IEnumerator`，`YieldInstruction`，`AsyncOperation`，`CustomYieldInstruction`时,自动切换到主线程

```c#
async void Async_WaitForSeconds()
{
	await new WaitForSeconds(1s);
    //1s after ...
}
//run
Async_WaitForSeconds();
```

调用方法直接异步执行

**协程代码**

```c#
IEnumerator Coroutine_WaitForSeconds()
{
	yield return new WaitForSeconds(1s);
    //1s after ... 
}
//run
StartCoroutine(Coroutine_WaitForSeconds());
```

调用 `StartCoroutine` 运行协程

**Web 请求**

```c#
async void WebRequest()
{
    UnityWebRequest request = UnityWebRequest.Get(url);
    await request.SendWebRequest();
    //... request.downloadHandler.text
}
```

## 返回值

### 异步返回值

```c#
async Task<int> Async_Add(int a, int b)
{
    await new WaitForSeconds(0.1f);
    return a + b;
}
```

**异步获取异步返回值**

```c#
async Async()
{
    int result = await Async_Add(1,2);
    //result = 3
}
```

**协程获取异步返回值**

```c#
IEnumerator Routine()
{
    var task = Async_Add(1, 2);
    yield return task.AsRoutine();
    //task.Result == 3
}
```



### **Wait** 等待异步值

```c#
int n = 0;
bool isDone = false;
Task.Delay(100)
    .ContinueWith((t) =>
		{
            n = 1;
            isDone = true;
        });

int result = await new Wait<int>((out int result) =>
	{
        result = n;
        return isDone;
    });
```

设置超时时间

超时后抛出 `TimeoutException` 异常

```c#
int result = await new Wait<int>((out int result) =>
	{
        result = n;
        return isDone;
    }).Timeout(1000); //超时时间 1s
```

超时后不触发异常

```c#
int result = await new Wait<int>((out int result) =>
	{
        result = n;
        return isDone;
    }).Timeout(1000, false); //超时时间 1s
```



### 协程返回值

Unity 协程不支持返回值，使用 `YieldReturn` 即可返回值

```c#
IEnumerator Coroutine_Add(int a, int b)
{
    yield return new YieldReturn(a + b); ;
}
```

**异步获取协程返回值**

两种方法获取

- await

```c#
async Async()
{
	var result = (int)await Coroutine_ReturnAdd(1, 2);
	//result == 3   
}
```

- Await

```c#
async Async()
{
    var result = Coroutine_ReturnAdd(1, 2).Await<int>().Result;
    //result == 3
}
```

**协程获取协程返回值**

```c#
IEnumerator Routine()
{
    var task = Coroutine_ReturnAdd(1, 2).Await<int>();
    yield return task.AsRoutine();
    //task.Result == 3
}
```



## 处理异常

```c#
Task.Run(() =>
	{
        throw new Exception();
    });
```

任务中抛出的异常默认会被忽略

### 延续任务获取异常

```c#
Task.Run(() =>
	{
        throw new Exception();
    }).ContinueWith(t =>
	{
        //t.Status == TaskStatus.Faulted
        //t.Exception
    });
```

### await 捕获异常

```c#
try
{
    await Task.Run(() =>
		{
            throw new Exception();
        });
}
catch (Exception ex)
{
    Debug.LogException(ex);
}
```



## 可等待对象

### 等待时间

```c#
await new WaitForTime(seconds);
```

### 等待

```c#
await new Wait(
    () =>
		{
            return isDone;
        });
```

**等待并返回值**

```c#
int result = await new Wait<int>(
    (out int result) =>
    {
        if (isDone) { 
	        result = n;
        }
        return isDone;
    });
```



### 等待值

直到 `value` 不为空

```c#
string result = await new WaitNotNull<string>(() => value);
```





## **编辑器**

依赖`com.unity.editorcoroutines` `Editor Coroutines` 包编辑器模拟运行协程，称为`编辑器主线程`，仅支持少部分 Unity 的协程等待对象，如 `IEnumerator`，`WaitForEndOfFrame`，`AsyncOperation`，`CustomYieldInstruction` 

**切换到编辑器主线程**

```c#
await new EditorMainThread();
```

### 等待对象

**等待时间**

编辑器等待时间使用 `WaitForTime`，不支持 `WaitForSeconds`，`WaitForSecondsRealtime`

```c#
await new WaitForTime(1s);
//1s after ...
```





> [Await 说明](Doc~/Await.md)