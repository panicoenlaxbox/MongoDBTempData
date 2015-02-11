using System;
using System.Web.Mvc;

namespace MongoDBTempData
{
    public delegate Guid GetUniqueId(ControllerContext controllerContext);
}