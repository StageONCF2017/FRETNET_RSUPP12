Imports System.Web.Security
Imports DotNetOpenAuth.AspNet
Imports Microsoft.AspNet.Membership.OpenAuth

Public Class RegisterExternalLogin
    Inherits System.Web.UI.Page

    Protected Property ProviderName As String
        Get
            Return If(DirectCast(ViewState("ProviderName"), String), String.Empty)
        End Get
        Private Set(value As String)
            ViewState("ProviderName") = value
        End Set
    End Property

    Protected Property ProviderDisplayName As String
        Get
            Return If(DirectCast(ViewState("PropertyProviderDisplayName"), String), String.Empty)
        End Get
        Private Set(value As String)
            ViewState("ProviderDisplayName") = value
        End Set
    End Property

    Protected Property ProviderUserId As String
        Get
            Return If(DirectCast(ViewState("ProviderUserId"), String), String.Empty)
        End Get

        Private Set(value As String)
            ViewState("ProviderUserId") = value
        End Set
    End Property

    Protected Property ProviderUserName As String
        Get
            Return If(DirectCast(ViewState("ProviderUserName"), String), String.Empty)
        End Get

        Private Set(value As String)
            ViewState("ProviderUserName") = value
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ProcessProviderResult()
        End If
    End Sub

    Protected Sub logIn_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        CreateAndLoginUser()
    End Sub

    Protected Sub cancel_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        RedirectToReturnUrl()
    End Sub

    Private Sub ProcessProviderResult()
        ' Traiter le résultat à partir d'un fournisseur d'authentification de la demande
        ProviderName = OpenAuth.GetProviderNameFromCurrentRequest()

        If String.IsNullOrEmpty(ProviderName) Then
            Response.Redirect(FormsAuthentication.LoginUrl)
        End If

        ' Générer l'URL de redirection pour la vérification OpenAuth
        Dim redirectUrl As String = "~/Account/RegisterExternalLogin"
        Dim returnUrl As String = Request.QueryString("ReturnUrl")
        If Not String.IsNullOrEmpty(returnUrl) Then
            redirectUrl &= "?ReturnUrl=" & HttpUtility.UrlEncode(returnUrl)
        End If

        ' Vérifier la charge utile OpenAuth
        Dim authResult As AuthenticationResult = OpenAuth.VerifyAuthentication(redirectUrl)
        ProviderDisplayName = OpenAuth.GetProviderDisplayName(ProviderName)
        If Not authResult.IsSuccessful Then
            Title = "Échec de la connexion externe"
            userNameForm.Visible = False
            
            providerMessage.Text = String.Format("Échec de la connexion externe {0}.", ProviderDisplayName)
            
            ' Pour afficher cette erreur, activer le traçage des pages dans web.config (<system.web><trace enabled="true"/></system.web>) et consulter ~/Trace.axd
            Trace.Warn("OpenAuth", String.Format("Une erreur est survenue lors de la vérification de l'authentification avec {0})", ProviderDisplayName), authResult.Error)
            Return
        End If

        ' L'utilisateur s'est bien connecté au fournisseur
        ' Vérifier si l'utilisateur est déjà inscrit localement
        If OpenAuth.Login(authResult.Provider, authResult.ProviderUserId, createPersistentCookie:=False) Then
            RedirectToReturnUrl()
        End If

        ' Stocker les détails du fournisseur dans ViewState
        ProviderName = authResult.Provider
        ProviderUserId = authResult.ProviderUserId
        ProviderUserName = authResult.UserName

        ' Enlever la chaîne de requête de l'action
        Form.Action = ResolveUrl(redirectUrl)

        If (User.Identity.IsAuthenticated) Then
            ' L'utilisateur est déjà authentifié, ajouter la connexion externe et rediriger vers l'URL de renvoi
            OpenAuth.AddAccountToExistingUser(ProviderName, ProviderUserId, ProviderUserName, User.Identity.Name)
            RedirectToReturnUrl()
        Else
            ' Il s'agit d'un nouvel utilisateur, demander quel est le nom de membre souhaité
            userName.Text = authResult.UserName
        End If
    End Sub

    Private Sub CreateAndLoginUser()
        If Not IsValid Then
            Return
        End If

        Dim createResult As CreateResult = OpenAuth.CreateUser(ProviderName, ProviderUserId, ProviderUserName, userName.Text)

        If Not createResult.IsSuccessful Then
            
            userNameMessage.Text = createResult.ErrorMessage
            
        Else
            ' Utilisateur créé & associé OK
            If OpenAuth.Login(ProviderName, ProviderUserId, createPersistentCookie:=False) Then
                RedirectToReturnUrl()
            End If
        End If
    End Sub

    Private Sub RedirectToReturnUrl()
        Dim returnUrl As String = Request.QueryString("ReturnUrl")
        If Not String.IsNullOrEmpty(returnUrl) And OpenAuth.IsLocalUrl(returnUrl) Then
            Response.Redirect(returnUrl)
        Else
            Response.Redirect("~/")
        End If
    End Sub
End Class