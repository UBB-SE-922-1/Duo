namespace Duo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Castle.Core.Configuration;
    using Duo.Helpers.Interfaces;
    using Duo.Helpers.ViewFactories;
    using Duo.Services;
    using Duo.ViewModels;
    using Duo.ViewModels.ExerciseViewModels;
    using Duo.ViewModels.Roadmap;
    using Duo.Views;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Repositories;
    using DuoClassLibrary.Repositories.Interfaces;
    using DuoClassLibrary.Repositories.Proxies;
    using DuoClassLibrary.Services;
    using DuoClassLibrary.Services.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Controls.Primitives;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;
    using Microsoft.UI.Xaml.Shapes;

    /// <summary>
    /// The main application class for Duo.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Provides access to the application's service provider for dependency injection.
        /// </summary>
        public static IServiceProvider ServiceProvider;

        /// <summary>
        /// Gets or sets the current user of the application.
        /// </summary>
        public static User? CurrentUser { get; set; }

        /// <summary>
        /// Gets the main application window.
        /// </summary>
        public static Window? MainAppWindow { get; private set; }

        /// <summary>
        /// Provides user-related operations and manages the current user.
        /// </summary>
        public static IUserService UserService;

        /// <summary>
        /// Provides helper methods for user operations.
        /// </summary>
        public static IUserHelperService UserHelperService;

        /// <summary>
        /// Provides access to user data storage.
        /// </summary>
        public static IUserRepository UserRepository;

        /// <summary>
        /// Provides access to post data storage.
        /// </summary>
        public static IPostRepository PostRepository;

        /// <summary>
        /// Provides access to hashtag data storage.
        /// </summary>
        public static IHashtagRepository HashtagRepository;

        /// <summary>
        /// Provides hashtag-related operations.
        /// </summary>
        public static IHashtagService HashtagService;

        /// <summary>
        /// Provides post-related operations.
        /// </summary>
        public static IPostService PostService;

        /// <summary>
        /// Provides category-related operations.
        /// </summary>
        public static ICategoryService CategoryService;

        /// <summary>
        /// Provides access to comment data storage.
        /// </summary>
        public static ICommentRepository CommentRepository;

        /// <summary>
        /// Provides comment-related operations.
        /// </summary>
        public static ICommentService CommentService;

        /// <summary>
        /// Provides search-related operations.
        /// </summary>
        public static SearchService SearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            UserRepository = new UserRepositoryProxy();
            UserHelperService = new UserHelperService(UserRepository);
            HashtagRepository = new HashtagRepositoryProxi();
            ICategoryRepository categoryRepository = new CategoryRepositoryProxi();
            PostRepository = new PostRepositoryProxi();
            HashtagService = new HashtagService(HashtagRepository, PostRepository);
            UserService = new UserService(UserHelperService);
            SearchService = new SearchService();
            PostService = new PostService(PostRepository, HashtagService, UserService, SearchService, HashtagRepository);
            CommentRepository = new CommentRepositoryProxi();
            CommentService = new CommentService(CommentRepository, PostRepository, UserService);
            CategoryService = new CategoryService(categoryRepository);

            var services = new ServiceCollection();
            this.ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configures dependency injection services.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        private void ConfigureServices(ServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IUserRepository, UserRepositoryProxy>();
            services.AddSingleton<IPostRepository, PostRepositoryProxi>();
            services.AddSingleton<ICommentRepository, CommentRepositoryProxi>();

            // Register services
            services.AddSingleton<IUserHelperService, UserHelperService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ICommentService, CommentService>();
            services.AddTransient<ILoginService, LoginService>();
            services.AddTransient<FriendsService>();
            services.AddTransient<SignUpService>();
            services.AddTransient<ProfileService>();
            services.AddTransient<LeaderboardService>();

            // Register view models
            services.AddTransient<LoginViewModel>();
            services.AddTransient<SignUpViewModel>();
            services.AddTransient<ResetPassViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<LeaderboardViewModel>();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var httpClient = new HttpClient(handler);

            services.AddSingleton<HttpClient>();

            // Course
            services.AddSingleton<ICourseServiceProxy>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new CourseServiceProxy(httpClient);
            });
            services.AddSingleton<ICourseService>(sp =>
            {
                var proxy = sp.GetRequiredService<ICourseServiceProxy>();
                return new CourseService(proxy);
            });

            // Coins
            services.AddSingleton<ICoinsServiceProxy>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new CoinsServiceProxy(httpClient);
            });
            services.AddSingleton<ICoinsService>(sp =>
            {
                var proxy = sp.GetRequiredService<ICoinsServiceProxy>();
                return new CoinsService(proxy);
            });

            // Exercise
            services.AddSingleton<IExerciseServiceProxy>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new ExerciseServiceProxy(httpClient);
            });
            services.AddSingleton<IExerciseService>(sp =>
            {
                var proxy = sp.GetRequiredService<IExerciseServiceProxy>();
                return new ExerciseService(proxy);
            });

            // Quiz
            services.AddSingleton<IQuizServiceProxy>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new QuizServiceProxy(httpClient);
            });
            services.AddSingleton<IQuizService>(sp =>
            {
                var proxy = sp.GetRequiredService<IQuizServiceProxy>();
                return new QuizService(proxy);
            });

            // Section
            services.AddSingleton<ISectionServiceProxy>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                return new SectionServiceProxy(httpClient);
            });
            services.AddSingleton<ISectionService>(sp =>
            {
                var proxy = sp.GetRequiredService<ISectionServiceProxy>();
                return new SectionService(proxy);
            });

            // Roadmap
            services.AddSingleton<IRoadmapServiceProxy>(
                sp =>
                {
                    var httpClient = sp.GetRequiredService<HttpClient>();
                    return new RoadmapServiceProxy(httpClient);
                });
            services.AddSingleton<IRoadmapService>(
                sp =>
                {
                    var proxy = sp.GetRequiredService<IRoadmapServiceProxy>();
                    return new RoadmapService(proxy);
                });

            services.AddSingleton<IExerciseViewFactory, ExerciseViewFactory>();

            services.AddTransient<FillInTheBlankExerciseViewModel>();
            services.AddTransient<MultipleChoiceExerciseViewModel>();
            services.AddTransient<AssociationExerciseViewModel>();
            services.AddTransient<ExerciseCreationViewModel>();
            services.AddTransient<QuizExamViewModel>();
            services.AddTransient<CreateQuizViewModel>();
            services.AddTransient<CreateSectionViewModel>();

            services.AddSingleton<RoadmapMainPageViewModel>();
            services.AddTransient<RoadmapSectionViewModel>();
            services.AddSingleton<RoadmapQuizPreviewViewModel>();
        }

        /// <summary>
        /// Handles the application launch.
        /// </summary>
        /// <param name="args">Launch arguments.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }

        /// <summary>
        /// Holds a reference to the application window (not used).
        /// </summary>
        private Window? window;
    }
}