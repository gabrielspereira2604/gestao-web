using GestaoWeb.Controllers;
using GestaoWeb.Models.Domain;
using GestaoWeb.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GestaoWeb.Tests.Controllers;

public class AccountControllerTests
{
    private static Mock<SignInManager<AppUser>> CreateSignInManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        var userManager = new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        var confirmation = new Mock<IUserConfirmation<AppUser>>();

        return new Mock<SignInManager<AppUser>>(
            userManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null,
            null,
            schemeProvider.Object,
            confirmation.Object);
    }

    [Fact]
    public void Login_Get_WhenAlreadySignedIn_RedirectsToHome()
    {
        var signInMock = CreateSignInManagerMock();
        signInMock.Setup(m => m.IsSignedIn(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                  .Returns(true);

        var controller = new AccountController(signInMock.Object);
        var result = controller.Login() as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Home", result.ControllerName);
    }

    [Fact]
    public void Login_Get_WhenNotSignedIn_ReturnsView()
    {
        var signInMock = CreateSignInManagerMock();
        signInMock.Setup(m => m.IsSignedIn(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                  .Returns(false);

        var controller = new AccountController(signInMock.Object);
        var result = controller.Login();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Login_Post_ValidCredentials_RedirectsToHome()
    {
        var signInMock = CreateSignInManagerMock();
        signInMock.Setup(m => m.PasswordSignInAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = new AccountController(signInMock.Object);
        var model = new LoginViewModel { Email = "user@test.com", Password = "Senha123" };

        var result = await controller.Login(model) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Home", result.ControllerName);
    }

    [Fact]
    public async Task Login_Post_InvalidCredentials_ReturnsViewWithModelError()
    {
        var signInMock = CreateSignInManagerMock();
        signInMock.Setup(m => m.PasswordSignInAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var controller = new AccountController(signInMock.Object);
        var model = new LoginViewModel { Email = "user@test.com", Password = "errada" };

        var result = await controller.Login(model) as ViewResult;

        Assert.NotNull(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Login_Post_InvalidModel_ReturnsView()
    {
        var signInMock = CreateSignInManagerMock();
        var controller = new AccountController(signInMock.Object);
        controller.ModelState.AddModelError("Email", "Required");

        var model = new LoginViewModel();
        var result = await controller.Login(model);

        Assert.IsType<ViewResult>(result);
        signInMock.Verify(m => m.PasswordSignInAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }
}
