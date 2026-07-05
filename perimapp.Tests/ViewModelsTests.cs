using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using perimapp.Data;
using perimapp.Models;
using perimapp.Services;
using perimapp.ViewModels;
using Xunit;


namespace perimapp.Tests
{
    // ================= MOCK IMPLEMENTATIONS FOR SERVICES =================

    public class MockNavigationService : INavigationService
    {
        public string? NavigatedTo { get; set; }
        public bool ModalPushed { get; set; }
        public bool ModalPopped { get; set; }

        public Task GoToAsync(string state)
        {
            NavigatedTo = state;
            return Task.CompletedTask;
        }

        public Task PopModalAsync()
        {
            ModalPopped = true;
            return Task.CompletedTask;
        }

        public Task PushModalAsync(Page page)
        {
            ModalPushed = true;
            return Task.CompletedTask;
        }
    }

    public class MockDialogService : IDialogService
    {
        public bool AlertShown { get; set; }
        public bool ConfirmResult { get; set; } = true;
        public string? PromptResult { get; set; }
        public (string FirstName, string LastName)? EditProfileResult { get; set; }
        public bool NotificationSettingsShown { get; set; }
        public string? ActionSheetResult { get; set; }

        public Task ShowAlertAsync(string title, string message, string buttonText = "OK")
        {
            AlertShown = true;
            return Task.CompletedTask;
        }

        public Task<bool> ShowConfirmAsync(string title, string message, string yesText = "Oui", string noText = "Non")
        {
            return Task.FromResult(ConfirmResult);
        }

        public Task<string?> ShowPromptAsync(string title, string message, string placeholder = "", string okText = "Valider", string cancelText = "Annuler")
        {
            return Task.FromResult(PromptResult);
        }

        public Task<(string FirstName, string LastName)?> ShowEditProfileAsync(string currentFirstName, string currentLastName)
        {
            return Task.FromResult(EditProfileResult);
        }

        public Task ShowNotificationSettingsAsync()
        {
            NotificationSettingsShown = true;
            return Task.CompletedTask;
        }

        public Task<string> ShowActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
        {
            return Task.FromResult(ActionSheetResult ?? cancel);
        }
    }

    public class MockDispatcherService : IDispatcherService
    {
        public void BeginInvokeOnMainThread(Action action)
        {
            action();
        }
    }

    public class MockPreferences : IPreferences
    {
        public Dictionary<string, object> Dict { get; } = new();

        public bool ContainsKey(string key, string? sharedName = null) => Dict.ContainsKey(key);
        public void Remove(string key, string? sharedName = null) => Dict.Remove(key);
        public void Clear(string? sharedName = null) => Dict.Clear();

        public void Set<T>(string key, T value, string? sharedName = null)
        {
            if (value == null) Dict.Remove(key);
            else Dict[key] = value;
        }

        public T Get<T>(string key, T defaultValue, string? sharedName = null)
        {
            if (Dict.TryGetValue(key, out var val))
            {
                return (T)val;
            }
            return defaultValue;
        }
    }

    // ================= UNIT TESTS =================

    public class ViewModelsTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly LocalProductService _localProductService;
        private readonly LocalUserService _localUserService;
        private readonly MockSecureStorage _mockSecureStorage;
        private readonly MockHttpMessageHandler _mockHttpHandler;
        private readonly ApiProductService _apiProductService;
        private readonly ApiProfileService _apiProfileService;
        private readonly AuthService _authService;
        private readonly SyncService _syncService;
        private readonly MockNavigationService _mockNavigationService;
        private readonly MockDialogService _mockDialogService;
        private readonly MockDispatcherService _mockDispatcherService;
        private readonly MockPreferences _mockPreferences;
        private readonly MockConnectivity _mockConnectivity;

        public ViewModelsTests()
        {
            _tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VmTestStorage_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);

            _localProductService = new LocalProductService(_tempDirectory);
            _localUserService = new LocalUserService(_tempDirectory);
            _mockSecureStorage = new MockSecureStorage();
            _mockHttpHandler = new MockHttpMessageHandler();

            var httpClient = new HttpClient(_mockHttpHandler);
            _apiProductService = new ApiProductService(httpClient, _mockSecureStorage);
            _apiProfileService = new ApiProfileService(httpClient, _mockSecureStorage);
            _authService = new AuthService(httpClient, _mockSecureStorage);

            _mockConnectivity = new MockConnectivity();
            _syncService = new SyncService(_apiProductService, _localProductService, _mockConnectivity);

            _mockNavigationService = new MockNavigationService();
            _mockDialogService = new MockDialogService();
            _mockDispatcherService = new MockDispatcherService();
            _mockPreferences = new MockPreferences();
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                    Directory.Delete(_tempDirectory, true);
            }
            catch { }
        }

        // 1. LoadingViewModel
        [Fact]
        public async Task LoadingViewModel_LoadApp_NavigatesToMainView_WhenTokenAndUserExist()
        {
            // Arrange
            var user = new UserProfileDetails { Id = 1, FirstName = "A", Email = "a@test.com", HomeCode = "H1" };
            await _localUserService.SaveUserAsync(user);
            await _mockSecureStorage.SetAsync("auth_token", "my_token");

            var vm = new LoadingViewModel(_mockNavigationService, _localUserService, _mockSecureStorage);

            // Act
            await vm.LoadAppCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("///MainView", _mockNavigationService.NavigatedTo);
        }

        [Fact]
        public async Task LoadingViewModel_LoadApp_NavigatesToStartingView_AndClears_WhenNoToken()
        {
            // Arrange
            var user = new UserProfileDetails { Id = 1, FirstName = "A", Email = "a@test.com", HomeCode = "H1" };
            await _localUserService.SaveUserAsync(user);

            var vm = new LoadingViewModel(_mockNavigationService, _localUserService, _mockSecureStorage);

            // Act
            await vm.LoadAppCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("///StartingView", _mockNavigationService.NavigatedTo);
            Assert.Null(await _mockSecureStorage.GetAsync("auth_token"));
            Assert.Null(await _localUserService.LoadUserAsync());
        }

        // 2. StartingViewModel
        [Fact]
        public async Task StartingViewModel_ShouldNavigateCorrectly()
        {
            var vm = new StartingViewModel(_mockNavigationService, _mockDialogService);

            await vm.LogInCommand.ExecuteAsync(null);
            Assert.Equal("LogInView", _mockNavigationService.NavigatedTo);

            await vm.SignUpCommand.ExecuteAsync(null);
            Assert.Equal("SignUpView", _mockNavigationService.NavigatedTo);
        }

        // 3. LogInViewModel
        [Fact]
        public async Task LogInViewModel_SignInSuccess_NavigatesCorrectly()
        {
            // Arrange
            _mockHttpHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                if (req.RequestUri!.AbsolutePath.Contains("sign-in"))
                {
                    response.Content = JsonContent.Create(new BetterAuthResponse { Token = "tok123" });
                }
                else if (req.RequestUri.AbsolutePath.Contains("FamilyInfos/me"))
                {
                    response.Content = JsonContent.Create(new UserProfileDetails { Id = 5, HomeCode = "H1", IsValidated = true, Email = "test@test.com" });
                }
                return Task.FromResult(response);
            };

            var vm = new LogInViewModel(
                _authService, 
                _apiProfileService, 
                _localUserService,
                _mockNavigationService,
                _mockDialogService,
                _mockSecureStorage,
                _mockPreferences)
            {
                EmailText = "test@test.com",
                PasswordText = "password"
            };

            // Act
            await vm.NextLogInCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("///MainView", _mockNavigationService.NavigatedTo);
            Assert.Equal("5", await _mockSecureStorage.GetAsync("user_id"));
            Assert.Equal(5, _mockPreferences.Get("mon_user_id", 0));
            Assert.Equal("H1", _mockPreferences.Get("mon_home_code", ""));
        }

        [Fact]
        public async Task LogInViewModel_SignInFail_ShowsPopup()
        {
            // Arrange
            _mockHttpHandler.Handler = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var vm = new LogInViewModel(
                _authService, 
                _apiProfileService, 
                _localUserService,
                _mockNavigationService,
                _mockDialogService,
                _mockSecureStorage,
                _mockPreferences)
            {
                EmailText = "test@test.com",
                PasswordText = "password"
            };

            // Act
            await vm.NextLogInCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_mockDialogService.AlertShown);
            Assert.Null(_mockNavigationService.NavigatedTo);
        }

        [Fact]
        public async Task LogInViewModel_Back_NavigatesBack()
        {
            var vm = new LogInViewModel(
                _authService, 
                _apiProfileService, 
                _localUserService,
                _mockNavigationService,
                _mockDialogService,
                _mockSecureStorage,
                _mockPreferences);

            await vm.BackLogInCommand.ExecuteAsync(null);
            Assert.Equal("..", _mockNavigationService.NavigatedTo);
        }

        // 4. SignUpViewModel
        [Fact]
        public async Task SignUpViewModel_SignUpSuccess_NavigatesCorrectly()
        {
            // Arrange
            _mockHttpHandler.Handler = req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                if (req.RequestUri!.AbsolutePath.Contains("sign-up"))
                {
                    response.Content = JsonContent.Create(new BetterAuthResponse { Token = "tok123" });
                }
                else if (req.RequestUri.AbsolutePath.Contains("FamilyInfos/me"))
                {
                    response.Content = JsonContent.Create(new UserProfileDetails { Id = 8, HomeCode = "H_NEW", IsValidated = false, Email = "test@test.com" });
                }
                return Task.FromResult(response);
            };

            var vm = new SignUpViewModel(
                _authService, 
                _apiProfileService, 
                _localUserService,
                _mockNavigationService,
                _mockDialogService,
                _mockSecureStorage,
                _mockPreferences)
            {
                EmailText = "test@test.com",
                PasswordText = "password",
                ConfirmPasswordText = "password",
                HomeCodeText = ""
            };

            // Act
            await vm.NextSignUpCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_mockDialogService.AlertShown); // VerificationPopup is shown
            Assert.Equal("EmailVerificationView", _mockNavigationService.NavigatedTo);
        }

        [Fact]
        public async Task SignUpViewModel_Back_NavigatesBack()
        {
            var vm = new SignUpViewModel(
                _authService, 
                _apiProfileService, 
                _localUserService,
                _mockNavigationService,
                _mockDialogService,
                _mockSecureStorage,
                _mockPreferences);

            await vm.BackSignUpCommand.ExecuteAsync(null);
            Assert.Equal("..", _mockNavigationService.NavigatedTo);
        }

        // 5. EmailVerificationViewModel
        [Fact]
        public async Task EmailVerificationViewModel_VerifySuccess_NavigatesToMain()
        {
            // Arrange
            _mockHttpHandler.Handler = req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            var vm = new EmailVerificationViewModel(
                _apiProfileService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService)
            {
                VerificationCode = "123456"
            };

            // Act
            await vm.VerifyCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_mockDialogService.AlertShown);
            Assert.Equal("///MainView", _mockNavigationService.NavigatedTo);
        }

        [Fact]
        public async Task EmailVerificationViewModel_GoBack_StopsTimerAndNavigatesBack()
        {
            var vm = new EmailVerificationViewModel(
                _apiProfileService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService);

            await vm.GoBackCommand.ExecuteAsync(null);
            Assert.Equal("..", _mockNavigationService.NavigatedTo);
        }

        // 6. MainViewModel
        [Fact]
        public async Task MainViewModel_LoadProducts_PopulatesAppDataProducts()
        {
            // Arrange
            var activeProduct = new ProductInfos { ProductUniqueId = "p1", Name = "Active Product", State = "Active" };
            var deletedProduct = new ProductInfos { ProductUniqueId = "p2", Name = "Deleted Product", State = "Deleted" };
            await _localProductService.SaveProductsAsync(new List<ProductInfos> { activeProduct, deletedProduct });

            var vm = new MainViewModel(
                _localProductService, 
                _syncService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService);

            // Act
            await vm.LoadProductsCommand.ExecuteAsync(null);

            // Assert
            Assert.Single(AppData.CurrentProducts);
            Assert.Equal("Active Product", AppData.CurrentProducts[0].Name);
            Assert.Equal(1, vm.DisplayedProductsCount);
        }

        [Fact]
        public async Task MainViewModel_NavigationCommands_TriggerCorrectRoutes()
        {
            // Arrange
            var vm = new MainViewModel(
                _localProductService, 
                _syncService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService);

            // Act & Assert
            await vm.ProfileIconClickedCommand.ExecuteAsync(null);
            Assert.Equal("ProfileView", _mockNavigationService.NavigatedTo);

            await vm.AddProductClickedCommand.ExecuteAsync(null);
            Assert.Equal("AddProductView", _mockNavigationService.NavigatedTo);

            await vm.LostProductsClickedCommand.ExecuteAsync(null);
            Assert.Equal("DeletedProductView", _mockNavigationService.NavigatedTo);

            var p = new ProductInfos { ProductUniqueId = "p123", Name = "Test" };
            await vm.ProductSelectedCommand.ExecuteAsync(p);
            Assert.Equal("DetailsView?ProductUniqueId=p123", _mockNavigationService.NavigatedTo);
        }

        [Fact]
        public async Task MainViewModel_Sort_RearrangesProducts()
        {
            // Arrange
            AppData.Clear();
            var p1 = new ProductInfos { Name = "P1", Dlc = DateOnly.FromDateTime(DateTime.Today.AddDays(5)) };
            var p2 = new ProductInfos { Name = "P2", Dlc = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };
            AppData.CurrentProducts.Add(p1);
            AppData.CurrentProducts.Add(p2);

            _mockDialogService.ActionSheetResult = "DLC (proche)";

            var vm = new MainViewModel(
                _localProductService, 
                _syncService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService);

            // Act
            await vm.SortButtonClickedCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("P2", AppData.CurrentProducts[0].Name); // 1 day remaining is closer than 5
            Assert.Equal("Tri: DLC (proche)", vm.SortButtonText);
        }

        // 7. DetailsViewModel
        [Fact]
        public async Task DetailsViewModel_LoadProductDetail_Fails_WhenProductInactive()
        {
            // Arrange
            AppData.Clear();
            var p = new ProductInfos { ProductUniqueId = "p1", Name = "Exp", State = "Deleted" };
            AppData.CurrentProducts.Add(p);

            var vm = new DetailsViewModel(
                _localProductService, 
                _localUserService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService)
            {
                ProductUniqueId = "p1"
            };

            // Act
            await vm.LoadProductDetailCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_mockDialogService.AlertShown);
            Assert.Equal("..", _mockNavigationService.NavigatedTo);
        }

        [Fact]
        public async Task DetailsViewModel_DeleteProduct_UpdatesStateAndNavigatesBack()
        {
            // Arrange
            AppData.Clear();
            var p = new ProductInfos { ProductUniqueId = "p1", Name = "Delete Me", State = "Active" };
            AppData.CurrentProducts.Add(p);
            await _localProductService.SaveProductsAsync(new List<ProductInfos> { p });

            _mockDialogService.ConfirmResult = true;

            var vm = new DetailsViewModel(
                _localProductService, 
                _localUserService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService)
            {
                ProductUniqueId = "p1",
                ProductDetail = p
            };

            // Act
            await vm.DeleteProductCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("..", _mockNavigationService.NavigatedTo);
            var loaded = await _localProductService.LoadProductsAsync();
            Assert.Equal("Deleted", loaded[0].State);
        }

        // 8. AddProductViewModel
        [Fact]
        public async Task AddProductViewModel_Quantity_IncrementDecrement()
        {
            var vm = new AddProductViewModel(
                _localProductService, 
                _localUserService, 
                _apiProductService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService,
                _mockConnectivity);

            Assert.Equal(1, vm.CurrentQuantity);

            vm.IncrementQuantityCommand.Execute(null);
            Assert.Equal(2, vm.CurrentQuantity);

            vm.DecrementQuantityCommand.Execute(null);
            Assert.Equal(1, vm.CurrentQuantity);
        }

        // 9. ModifyProductViewModel
        [Fact]
        public async Task ModifyProductViewModel_SaveProduct_SavesAndNavigates()
        {
            // Arrange
            var product = new ProductInfos { ProductUniqueId = "p1", Name = "Initial", Quantity = 2, State = "Active" };
            var vm = new ModifyProductViewModel(
                _localProductService,
                _mockNavigationService,
                _mockDialogService)
            {
                CurrentProduct = product,
                CurrentCustomName = "Modified Name",
                CurrentQuantity = 5
            };

            // Act
            await vm.SaveProductCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_mockDialogService.AlertShown);
            Assert.Equal("Modified Name", product.CustomName);
            Assert.Equal(5, product.Quantity);
            Assert.Contains("DetailsView", _mockNavigationService.NavigatedTo!);
        }

        // 10. DeletedProductViewModel
        [Fact]
        public async Task DeletedProductViewModel_Restore_ChangesStateToActive()
        {
            // Arrange
            var product = new ProductInfos { ProductUniqueId = "p1", Name = "Trash", State = "Deleted" };
            await _localProductService.SaveProductsAsync(new List<ProductInfos> { product });

            var vm = new DeletedProductViewModel(_localProductService, _mockDialogService);
            vm.Products.Add(product);

            _mockDialogService.ConfirmResult = true;

            // Act
            await vm.RestoreCommand.ExecuteAsync(product);

            // Assert
            var loaded = await _localProductService.LoadProductsAsync();
            Assert.Equal("Active", loaded[0].State);
            Assert.Empty(vm.Products);
        }

        // 11. ScannerViewModel
        [Fact]
        public async Task ScannerViewModel_Cancel_PopsModal()
        {
            var vm = new ScannerViewModel(_mockNavigationService, _mockDispatcherService);
            await vm.CancelCommand.ExecuteAsync(null);
            Assert.True(_mockNavigationService.ModalPopped);
        }

        // 12. ProfileViewModel
        [Fact]
        public async Task ProfileViewModel_LoadProfileData_UpdatesUI()
        {
            // Arrange
            var user = new UserProfileDetails { Id = 10, FirstName = "Jean", LastName = "Dupont", HomeCode = "FAM123", LostProductCount = 3, Email = "jean.dupont@test.com" };
            await _localUserService.SaveUserAsync(user);

            var vm = new ProfileViewModel(
                _localUserService,
                _localProductService,
                _apiProfileService,
                _authService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService,
                _mockConnectivity);

            // Act
            await vm.LoadProfileDataCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("Jean Dupont", vm.UserName);
            Assert.Equal("FAM123", vm.FamilyCode);
            Assert.Equal(3, vm.LostProductsCount);
        }

        [Fact]
        public async Task ProfileViewModel_Logout_ClearsDataAndNavigates()
        {
            // Arrange
            var vm = new ProfileViewModel(
                _localUserService,
                _localProductService,
                _apiProfileService,
                _authService,
                _mockNavigationService,
                _mockDialogService,
                _mockDispatcherService,
                _mockConnectivity);

            // Act
            await vm.LogoutCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal("///StartingView", _mockNavigationService.NavigatedTo);
        }
    }
}
