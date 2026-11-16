using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using study_room_booking.Controllers;
using study_room_booking.DAL;
using study_room_booking.Models;

namespace study_room_booking.Tests.Controllers;

public class AccountControllerTests
{
    // Tests that valid credentials sign in the user and redirect to a local return URL.
    // Arrange: configure SignInManager to accept the credentials and set a local return URL.
    // Act: submit the login form through the controller.
    // Assert: verify the user is redirected to that URL and PasswordSignInAsync is called.
    [Fact]
    public async Task Login_WithValidCredentials_RedirectsToReturnUrl()
    {
        // Arrange
        var userStore = Substitute.For<IUserStore<IdentityUser>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        var passwordHasher = Substitute.For<IPasswordHasher<IdentityUser>>();
        var userValidators = Array.Empty<IUserValidator<IdentityUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<IdentityUser>>();
        var normalizer = Substitute.For<ILookupNormalizer>();
        var errorDescriber = new IdentityErrorDescriber();
        var services = Substitute.For<IServiceProvider>();
        var userManagerLogger = Substitute.For<ILogger<UserManager<IdentityUser>>>();

        var userManager = Substitute.For<UserManager<IdentityUser>>(
            userStore,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            normalizer,
            errorDescriber,
            services,
            userManagerLogger);

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>();
        var signInLogger = Substitute.For<ILogger<SignInManager<IdentityUser>>>();
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        var userConfirmation = Substitute.For<IUserConfirmation<IdentityUser>>();

        var signInManager = Substitute.For<SignInManager<IdentityUser>>(
            userManager,
            httpContextAccessor,
            claimsFactory,
            options,
            signInLogger,
            schemeProvider,
            userConfirmation);

        signInManager
            .PasswordSignInAsync("alex", "CorrectPassword1!", false, false)
            .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

        var studentRepository = Substitute.For<IStudentRepository>();
        var controller = new AccountController(
            userManager,
            signInManager,
            studentRepository);

        var urlHelper = Substitute.For<IUrlHelper>();
        urlHelper.IsLocalUrl("/my-page").Returns(true);
        controller.Url = urlHelper;

        var model = new LoginViewModel
        {
            Username = "alex",
            Password = "CorrectPassword1!",
            RememberMe = false,
            ReturnUrl = "/my-page"
        };

        // Act
        var result = await controller.Login(model);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/my-page", redirect.Url);

        await signInManager.Received(1)
            .PasswordSignInAsync("alex", "CorrectPassword1!", false, false);
    }

    // Tests that successful registration creates a student profile, signs in the user,
    // and redirects to the home page.
    // Arrange: configure Identity and the student repository to succeed.
    // Act: submit valid registration details.
    // Assert: verify the student profile is linked to the new user and the user is signed in.
    [Fact]
    public async Task Register_WithValidDetails_CreatesStudentAndRedirectsHome()
    {
        // Arrange
        var userStore = Substitute.For<IUserStore<IdentityUser>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        var passwordHasher = Substitute.For<IPasswordHasher<IdentityUser>>();
        var userValidators = Array.Empty<IUserValidator<IdentityUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<IdentityUser>>();
        var normalizer = Substitute.For<ILookupNormalizer>();
        var errorDescriber = new IdentityErrorDescriber();
        var services = Substitute.For<IServiceProvider>();
        var userManagerLogger = Substitute.For<ILogger<UserManager<IdentityUser>>>();

        var userManager = Substitute.For<UserManager<IdentityUser>>(
            userStore,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            normalizer,
            errorDescriber,
            services,
            userManagerLogger);

        userManager.CreateAsync(Arg.Any<IdentityUser>(), "Password123!")
            .Returns(callInfo =>
            {
                callInfo.Arg<IdentityUser>().Id = "user-123";
                return Task.FromResult(IdentityResult.Success);
            });

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>();
        var signInLogger = Substitute.For<ILogger<SignInManager<IdentityUser>>>();
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        var userConfirmation = Substitute.For<IUserConfirmation<IdentityUser>>();

        var signInManager = Substitute.For<SignInManager<IdentityUser>>(
            userManager,
            httpContextAccessor,
            claimsFactory,
            options,
            signInLogger,
            schemeProvider,
            userConfirmation);

        signInManager.SignInAsync(Arg.Any<IdentityUser>(), false)
            .Returns(Task.CompletedTask);

        Student? savedStudent = null;
        var studentRepository = Substitute.For<IStudentRepository>();
        studentRepository
            .Create(Arg.Do<Student>(student => savedStudent = student))
            .Returns(true);

        var controller = new AccountController(
            userManager,
            signInManager,
            studentRepository);

        var model = new RegisterViewModel
        {
            Username = "alex",
            Email = "alex@example.com",
            FullName = "Alex Student",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var result = await controller.Register(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);

        await studentRepository.Received(1).Create(Arg.Any<Student>());

        Assert.NotNull(savedStudent);
        Assert.Equal("Alex Student", savedStudent.FullName);
        Assert.Equal("user-123", savedStudent.IdentityUserId);

        await signInManager.Received(1)
            .SignInAsync(
                Arg.Is<IdentityUser>(user => user.Id == "user-123"),
                false);
    }

    // Tests that invalid credentials show an error and do not redirect the user.
    // Arrange: configure SignInManager to reject the credentials.
    // Act: submit the login form through the controller.
    // Assert: verify the login form and error are returned.
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsLoginViewWithError()
    {
        // Arrange
        var userStore = Substitute.For<IUserStore<IdentityUser>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        var passwordHasher = Substitute.For<IPasswordHasher<IdentityUser>>();
        var userValidators = Array.Empty<IUserValidator<IdentityUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<IdentityUser>>();
        var normalizer = Substitute.For<ILookupNormalizer>();
        var errorDescriber = new IdentityErrorDescriber();
        var services = Substitute.For<IServiceProvider>();
        var userManagerLogger = Substitute.For<ILogger<UserManager<IdentityUser>>>();

        var userManager = Substitute.For<UserManager<IdentityUser>>(
            userStore,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            normalizer,
            errorDescriber,
            services,
            userManagerLogger);

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>();
        var signInLogger = Substitute.For<ILogger<SignInManager<IdentityUser>>>();
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        var userConfirmation = Substitute.For<IUserConfirmation<IdentityUser>>();

        var signInManager = Substitute.For<SignInManager<IdentityUser>>(
            userManager,
            httpContextAccessor,
            claimsFactory,
            options,
            signInLogger,
            schemeProvider,
            userConfirmation);

        signInManager
            .PasswordSignInAsync("alex", "WrongPassword1!", false, false)
            .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

        var studentRepository = Substitute.For<IStudentRepository>();
        var controller = new AccountController(
            userManager,
            signInManager,
            studentRepository);

        var urlHelper = Substitute.For<IUrlHelper>();
        urlHelper.IsLocalUrl("/my-page").Returns(true);
        controller.Url = urlHelper;

        var model = new LoginViewModel
        {
            Username = "alex",
            Password = "WrongPassword1!",
            RememberMe = false,
            ReturnUrl = "/my-page"
        };

        // Act
        var result = await controller.Login(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.Contains(
            controller.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage == "Invalid login attempt.");

        await signInManager.Received(1)
            .PasswordSignInAsync("alex", "WrongPassword1!", false, false);
    }

    // Tests that a failed student-profile save rolls back the newly created Identity user.
    // Arrange: configure Identity creation to succeed and student-profile creation to fail.
    // Act: submit valid registration details.
    // Assert: verify the Identity user is deleted, an error is returned, and the user is not signed in.
    [Fact]
    public async Task Register_WhenStudentCannotBeSaved_DeletesIdentityUser()
    {
        // Arrange
        var userStore = Substitute.For<IUserStore<IdentityUser>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        var passwordHasher = Substitute.For<IPasswordHasher<IdentityUser>>();
        var userValidators = Array.Empty<IUserValidator<IdentityUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<IdentityUser>>();
        var normalizer = Substitute.For<ILookupNormalizer>();
        var errorDescriber = new IdentityErrorDescriber();
        var services = Substitute.For<IServiceProvider>();
        var userManagerLogger = Substitute.For<ILogger<UserManager<IdentityUser>>>();

        var userManager = Substitute.For<UserManager<IdentityUser>>(
            userStore,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            normalizer,
            errorDescriber,
            services,
            userManagerLogger);

        userManager.CreateAsync(Arg.Any<IdentityUser>(), "Password123!")
            .Returns(callInfo =>
            {
                callInfo.Arg<IdentityUser>().Id = "user-123";
                return Task.FromResult(IdentityResult.Success);
            });

        userManager.DeleteAsync(Arg.Any<IdentityUser>())
            .Returns(Task.FromResult(IdentityResult.Success));

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<IdentityUser>>();
        var signInLogger = Substitute.For<ILogger<SignInManager<IdentityUser>>>();
        var schemeProvider = Substitute.For<IAuthenticationSchemeProvider>();
        var userConfirmation = Substitute.For<IUserConfirmation<IdentityUser>>();

        var signInManager = Substitute.For<SignInManager<IdentityUser>>(
            userManager,
            httpContextAccessor,
            claimsFactory,
            options,
            signInLogger,
            schemeProvider,
            userConfirmation);

        var studentRepository = Substitute.For<IStudentRepository>();
        studentRepository.Create(Arg.Any<Student>()).Returns(false);

        var controller = new AccountController(
            userManager,
            signInManager,
            studentRepository);

        var model = new RegisterViewModel
        {
            Username = "alex",
            Email = "alex@example.com",
            FullName = "Alex Student",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var result = await controller.Register(model);

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Contains(
            controller.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage ==
                "Could not save the student profile. Please try again.");

        await userManager.Received(1).DeleteAsync(
            Arg.Is<IdentityUser>(user => user.Id == "user-123"));

        await signInManager.DidNotReceive()
            .SignInAsync(Arg.Any<IdentityUser>(), Arg.Any<bool>());
    }
}
