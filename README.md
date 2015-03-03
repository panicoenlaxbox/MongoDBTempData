MongoDBTempData
===============

Proveedor personalizado de TempData basado en MongoDB

El código está basado en los posts:

[TempData basado en MongoDB en ASP.NET MVC](http://panicoenlaxbox.blogspot.com.es/2014/05/tempdata-basado-en-mongodb-en-aspnet-mvc.html)

[TempData basado en MongoDB (II)](http://panicoenlaxbox.blogspot.com.es/2014/06/tempdata-basado-en-mongodb-ii.html)

En mi caso he utilizado Unity para hacerlo funcionar dentro del pipeline de ASP.NET MVC, pero cualquier otro IoC Container serviría igualmente

    container.RegisterType<ITempDataProvider, MongoDBTempDataProvider>(new InjectionConstructor(
        ConfigurationManager.AppSettings["MongoDBTempDataProviderConnectionString"],
        ConfigurationManager.AppSettings["MongoDBTempDataProviderDatabaseName"],
        ConfigurationManager.AppSettings["MongoDBTempDataProviderCollectionName"],
        getUniqueId));

Será nuestra responsabilidad crear una instancia del delegado GetUniqueId para pasar al proveedor un valor único de tipo Guid que servirá para identificar a los distintos usuarios:

En el ejemplo se ha optado por guardar este valor en una cookie:

    GetUniqueId getUniqueId = controllerContext =>
    {
        var cookie = controllerContext.HttpContext.Request.Cookies["UniqueId"];
        return new Guid(cookie.Value);
    };

Para establecer la cookie se ha agregado el siguiente código en Global.asax

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
        var cookie = Request.Cookies["UniqueId"];
        if (cookie != null)
        {
            return;
        }
        cookie = new HttpCookie("UniqueId")
        {
            HttpOnly = true,
            Value = Guid.NewGuid().ToString()
        };
        HttpContext.Current.Response.Cookies.Add(cookie);
    }

En el ejemplo, ambos métodos están finalmente incluidos en una clase MongoDBTempDataUtilities, siéntete libre de usarla o crear tu propia implementación para persistir y recuperar el valor de UniqueId.