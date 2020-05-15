using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MSPMY_IntroToFaceApi
{
    public static class FacialRecognitionService
    {
        #region Constant Fields
        const string _personGroupId = "persongroupid";
        const string _personGroupName = "Facial Recognition Login Group";
        readonly static Lazy<FaceClient> _faceApiClientHolder = new Lazy<FaceClient>(() =>
             new FaceClient(new ApiKeyServiceClientCredentials("0d59f309e10049ddb35a9cee7206d13e")) { Endpoint = "https://asadqbfaceapi.cognitiveservices.azure.com/"});
        #endregion

        #region Fields
        static int _networkIndicatorCount = 0;
        #endregion

        #region Properties
        static FaceClient FaceApiClient => _faceApiClientHolder.Value;
        #endregion

        #region Methods
        public static async Task RemoveExistingFace(Guid userId)
        {
            try
            {
                await FaceApiClient.PersonGroupPerson.DeleteAsync(_personGroupId, userId);
            }
            catch (APIErrorException e) when (e.Response.StatusCode.Equals(HttpStatusCode.NotFound))
            {
                await App.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
            }
        }

        public static async Task<Guid> AddNewFace(string username, Stream photo)
        {
            try
            {
                await CreatePersonGroup();

                var createPersonResult = await FaceApiClient.PersonGroupPerson.CreateAsync(_personGroupId, username);

                var faceResult = await FaceApiClient.PersonGroupPerson.AddFaceFromStreamAsync(_personGroupId, createPersonResult.PersonId, photo);

                var trainingStatus = await TrainPersonGroup(_personGroupId);
                if (trainingStatus.Status.Equals(TrainingStatusType.Failed))
                    throw new Exception(trainingStatus.Message);

                return faceResult.PersistedFaceId;
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
                return Guid.Empty;
            }
        }

        public static async Task<bool> IsFaceIdentified(string username, Stream photo)
        {
            try
            {
                var personGroupListTask = FaceApiClient.PersonGroupPerson.ListAsync(_personGroupId);

                var facesDetected = await FaceApiClient.Face.DetectWithStreamAsync(photo);
                var faceDetectedIds = facesDetected.Select(x => x.FaceId ?? new Guid()).ToArray();

                var facesIdentified = await FaceApiClient.Face.IdentifyAsync(faceDetectedIds, _personGroupId);

                var candidateList = facesIdentified.SelectMany(x => x.Candidates).ToList();

                var personGroupList = await personGroupListTask;

                var matchingUsernamePersonList = personGroupList.Where(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));

                return candidateList.Select(x => x.PersonId).Intersect(matchingUsernamePersonList.Select(y => y.PersonId)).Any();
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
                return false;
            }
        }

        static async Task CreatePersonGroup()
        {
            try
            {
                await FaceApiClient.PersonGroup.CreateAsync(_personGroupId, _personGroupName);
            }
            catch (APIErrorException e) when (e.Response.StatusCode.Equals(HttpStatusCode.Conflict))
            {
                await App.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
            }
        }

        static async Task<TrainingStatus> TrainPersonGroup(string personGroupId)
        {
            TrainingStatus trainingStatus;

            await FaceApiClient.PersonGroup.TrainAsync(personGroupId);

            do
            {
                trainingStatus = await FaceApiClient.PersonGroup.GetTrainingStatusAsync(_personGroupId);
            }
            while (!(trainingStatus.Status is TrainingStatusType.Failed || trainingStatus.Status is TrainingStatusType.Succeeded));

            return trainingStatus;
        }
        #endregion
    }
}
