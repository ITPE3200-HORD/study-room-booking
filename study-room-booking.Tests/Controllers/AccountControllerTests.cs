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
}