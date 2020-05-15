using FacialRecognitionLogin;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MSPMY_IntroToFaceApi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignUpPage : ContentPage
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        private async void TakePhoto_Clicked(object sender, EventArgs e)
        {
            string user = usernameEntry.Text;
            string pass = passwordEntry.Text;

            var option = await DisplayActionSheet(null, "Cancel", null, "Take Photo", "Gallery");

            if (string.IsNullOrEmpty(option))
                return;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                await DisplayAlert("Error", "Please complete the username and password fields first.", "Ok");
                return;
            }

            if (option.Equals("Take Photo")) //Take photo
            {
                //Take photo
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("Error", "No camera avaialble!", "Ok");
                    return;
                }

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                    Directory = "User Images",
                    Name = "userImage"
                });

                if (file == null)
                    return;

                //Send to Azure Face API to train.
                Guid trainingGuid = new Guid();
                trainingGuid = await FacialRecognitionService.AddNewFace(user, file.GetStream());

                //Then if training is successful,
                trainingDone.IsChecked = trainingGuid != Guid.Empty; //The guid will be not empty if the training is done

                file.Dispose();
            }
            else if (option.Equals("Gallery")) //Select from gallery
            {
                //Open gallery
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("Error", "Permission not granted to photos!", "Ok");
                    return;
                }

                var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });


                if (file == null)
                    return;

                //Send to Azure Face API to train.
                Guid trainingGuid = new Guid();
                trainingGuid = await FacialRecognitionService.AddNewFace(user, file.GetStream());

                //Then if training is successful,
                trainingDone.IsChecked = trainingGuid != Guid.Empty; //The guid will be not empty if the training is done

                file.Dispose();
            }
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            string user = usernameEntry.Text;
            string pass = passwordEntry.Text;

            //Ensure the fields are all filled out.
            if (string.IsNullOrEmpty(user)|| string.IsNullOrEmpty(pass) || !trainingDone.IsChecked)
            {
                await DisplayAlert("Error", "Please complete all fields.", "Ok");
                return;
            }

            //Store into secure storage.
            await SecureStorageService.SaveLogin(user, pass);

            //Inform the user that sign up is done and return to login page.
            await DisplayAlert("Success", "You have been signed up. Returning you to login page...", "Ok");
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}