
Public Class Global_asax
    Inherits HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Se déclenche lorsque l'application est démarrée
        AuthConfig.RegisterOpenAuth()
        RouteConfig.RegisterRoutes(RouteTable.Routes)
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Se déclenche au début de chaque demande
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Se déclenche lors d'une tentative d'authentification de l'utilisation
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Se déclenche lorsqu'une erreur se produit
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Se déclenche lorsque l'application se termine
    End Sub
End Class