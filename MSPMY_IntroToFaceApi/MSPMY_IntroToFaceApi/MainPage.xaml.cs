using FacialRecognitionLogin;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MSPMY_IntroToFaceApi
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Login_Clicked(object sender, EventArgs e)
        {
            string user = usernameEntry.Text;
            string pass = passwordEntry.Text;

            //Ensure the fields are all filled out.
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                await DisplayAlert("Error", "Please complete all fields.", "Ok");
                return;
            }

            //Ensure taking photo is enabled.
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("Error", "No camera avaialble!", "Ok");
                return;
            }

            //Take photo.
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                Directory = "User Images",
                Name = "userImage"
            });

            //If no photo is taken, do nothing more.
            if (file == null)
                return;

            var loginsuccess = await SecureStorageService.IsLoginCorrect(user, pass);
            if(!loginsuccess)
            {
                await DisplayAlert("Failure", "Incorrect login details.", "Ok");
                return;
            }

            //Check with Azure Face API if the face is recognised.
            var faceRecognised = await FacialRecognitionService.IsFaceIdentified(user, file.GetStream());
            if (faceRecognised) //If recognised
                await DisplayAlert("Success", "You have logged in.", "Ok");
            else
                await DisplayAlert("Failure", "Face not recognised.", "Ok");

            file.Dispose();
        }

        private void SignUp_Clicked(object sender, EventArgs e)
        {
            App.Current.MainPage.Navigation.PushAsync(new SignUpPage());
        }
    }
}
