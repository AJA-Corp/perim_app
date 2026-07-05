using System;
using Microsoft.Maui.ApplicationModel;

namespace perimapp.Services
{
    public class DispatcherService : IDispatcherService
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}
