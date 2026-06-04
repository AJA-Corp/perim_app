using Microsoft.Maui.Networking;
using System;
using System.Threading.Tasks;
using System.Linq;
using perimapp.Models;

namespace perimapp.Services
{
    public class SyncService
    {
        private readonly ApiProductService _apiService;
        private readonly LocalProductService _localDb;

        private bool _isSyncing = false;

        public SyncService(ApiProductService apiService, LocalProductService localDb)
        {
            _apiService = apiService;
            _localDb = localDb;

            Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        }

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                _ = Task.Run(async () => await ProcessSyncAsync());
            }
        }

        public async Task ProcessSyncAsync()
        {
            if (_isSyncing || Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return;

            try
            {
                _isSyncing = true;
                System.Diagnostics.Debug.WriteLine("[SYNC] Lancement de la synchronisation...");

                var pendingItems = await _localDb.GetPendingSyncProductsAsync();

                if (pendingItems.Any())
                {
                    bool success = await _apiService.SyncOfflineProductsAsync(pendingItems);

                    if (success)
                    {
                        foreach (var product in pendingItems)
                        {
                            if (product.SyncState == SyncState.PendingDelete)
                                await _localDb.HardDeleteProductAsync(product);
                            else
                            {
                                product.SyncState = SyncState.Synced;
                                await _localDb.UpdateProductLocalAsync(product);
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("[SYNC] Téléchargement des données du serveur...");

                var serverProducts = await _apiService.GetMyInventoryAsync();

                if (serverProducts != null)
                {
                    foreach (var p in serverProducts)
                    {
                        p.SyncState = SyncState.Synced;
                    }

                    await _localDb.SaveProductsAsync(serverProducts);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SYNC] Crash pendant la synchro : {ex.Message}");
            }
            finally
            {
                _isSyncing = false;
                System.Diagnostics.Debug.WriteLine("[SYNC] Synchronisation terminée.");
            }
        }
    }
}