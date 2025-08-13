using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class LoginRegister : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected string GetRandomBackgroundImage()
        {
            string[] imagenes = {
                "Images/1- IVC.jpg",
                "Images/2- IVC.jpg",
                "Images/3- IVC.jpg",
                "Images/4- IVC.jpg",
                "Images/5- IVC.jpg",
                "Images/6- IVC.jpg",
                "Images/7- IVC.jpg",
                "Images/8- IVC.jpg",
                "Images/9- IVC.jpg",
                "Images/10- IVC.jpg",
                "Images/11- IVC.jpg",
                "Images/12- IVC.jpg",
                "Images/13- IVC.jpg",
                "Images/14- IVC.jpg",
                "Images/15- IVC.jpg",
                "Images/16- IVC.jpg",
                "Images/17- IVC.jpg",
                "Images/18- IVC.jpg",
                "Images/19- IVC.jpg",
                "Images/20- IVC.jpg",
                "Images/21- IVC.jpg",
                "Images/22- IVC.jpg",
                "Images/23- IVC.jpg"
            };

            Random random = new Random();
            int indiceAleatorio = random.Next(0, imagenes.Length);

            return imagenes[indiceAleatorio];
        }
    }
}