using System;

namespace perimapp.Services
{
    public interface IDispatcherService
    {
        void BeginInvokeOnMainThread(Action action);
    }
}
