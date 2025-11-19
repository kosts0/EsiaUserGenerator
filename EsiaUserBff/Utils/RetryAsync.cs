namespace EsiaUserGenerator.Utils;

public static class RetryAsync
{
    public static async Task<bool> WhileFalse(Func<Task<bool>> action, 
        int retries = 3, TimeSpan? interval = null, bool throwOnMaxRetryCount = true, System.Exception? maxRetryCountException = null)
    {
        maxRetryCountException ??= new System.Exception("Превышено количество попыток Retry");
        interval ??= TimeSpan.FromSeconds(1);
        for (int i = 0; i < retries; i++)
        {
            var result = await action.Invoke();
            if (result) return true;
        }

        if (throwOnMaxRetryCount)
        {
            throw maxRetryCountException;
        }
        return false;
    }
    public static async Task<T?> WhileNull<T>(Func<Task<T?>> action, 
        int retries = 3, TimeSpan? interval = null, bool throwOnMaxRetryCount = true, System.Exception? maxRetryCountException = null)
    {
        maxRetryCountException ??= new System.Exception("Превышено количество попыток Retry");
        interval ??= TimeSpan.FromSeconds(1);
        for (int i = 0; i < retries; i++)
        {
            var result = await action.Invoke();
            if (result != null) return result;
            await Task.Delay(interval.Value);
        }

        if (throwOnMaxRetryCount)
        {
            throw maxRetryCountException;
        }

        return default(T);
    }
}