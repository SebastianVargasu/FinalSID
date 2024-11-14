using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class MatchScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;   // Para mostrar tu nombre de usuario
    [SerializeField] private TMP_Text friendNameText;   // Para mostrar el nombre de tu amigo cuando se emparejan

    private string userId;
    private DatabaseReference playersRef;
    private DatabaseReference friendsRef;

    private void OnEnable()
    {
        userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        playersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("players");
        friendsRef = playersRef.Child(userId).Child("friends");

        // Mostrar el nombre de usuario del jugador en la pantalla de emparejamiento
        playerNameText.text = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;

        // Iniciar la cola de emparejamiento y escuchar cambios en amigos en tiempo real
        JoinMatchingQueue();
        ListenForFriendStatusChanges();
        ListenForMatchedFriendNameChanges();
    }

    private async void JoinMatchingQueue()
    {
        // Marcar al jugador como en espera
        await playersRef.Child(userId).Child("status").SetValueAsync("waiting");
        Debug.Log("Jugador en espera para emparejamiento.");
    }

    private void ListenForFriendStatusChanges()
    {
        friendsRef.ChildAdded += HandleFriendStatusChange;
        friendsRef.ChildChanged += HandleFriendStatusChange;
    }

    private void ListenForMatchedFriendNameChanges()
    {
        // Escuchar los cambios en el nombre del amigo emparejado
        playersRef.Child(userId).Child("matchedFriendName").ValueChanged += HandleMatchedFriendNameChange;
    }

    private async void HandleFriendStatusChange(object sender, ChildChangedEventArgs args)
    {
        string friendId = args.Snapshot.Key;
        DataSnapshot statusSnapshot = await playersRef.Child(friendId).Child("status").GetValueAsync();

        // Verificar si el amigo está en la cola de emparejamiento waiting
        if (statusSnapshot.Exists && statusSnapshot.Value.ToString() == "waiting")
        {
            Debug.Log($"Emparejado con {friendId}");

            // Obtener el nombre del amigo y almacenar en ambos perfiles
            string friendName = (await playersRef.Child(friendId).Child("username").GetValueAsync()).Value.ToString();

            // Almacenar el ID y nombre del amigo en ambos jugadores
            await playersRef.Child(userId).Child("matchedFriendId").SetValueAsync(friendId);
            await playersRef.Child(userId).Child("matchedFriendName").SetValueAsync(friendName);
            
            await playersRef.Child(friendId).Child("matchedFriendId").SetValueAsync(userId);
            await playersRef.Child(friendId).Child("matchedFriendName").SetValueAsync(playerNameText.text);

            // Incrementar el matchCount en ambos jugadores
            await IncrementMatchCount(userId);
            await IncrementMatchCount(friendId);

            // Cambiar el estado a playing para ambos jugadores e iniciar el juego
            await StartGameWithFriend(friendId, friendName);

            // Detener la escucha de cambios una vez emparejado
            StopListeningForFriendStatusChanges();
        }
        else
        {
            friendNameText.text = "Esperando...";
        }
    }

    private async Task StartGameWithFriend(string friendId, string friendName)
    {
        Debug.Log($"Iniciando juego con {friendId}");

        // Mostrar el nombre del amigo en la UI para ambos jugadores
        friendNameText.text = friendName;

        // Cambiar el estado de ambos jugadores a playing
        await playersRef.Child(userId).Child("status").SetValueAsync("playing");
        await playersRef.Child(friendId).Child("status").SetValueAsync("playing");

        // Aqui puedes añadir logica para iniciar el juego, como cargar una escena o habilitar UI
    }

    private async Task IncrementMatchCount(string playerId)
    {
        DatabaseReference matchCountRef = playersRef.Child(playerId).Child("matchCount");
        var matchCountSnapshot = await matchCountRef.GetValueAsync();
        int currentMatchCount = matchCountSnapshot.Exists ? int.Parse(matchCountSnapshot.Value.ToString()) : 0;
        await matchCountRef.SetValueAsync(currentMatchCount + 1);
    }

    private async void HandleMatchedFriendNameChange(object sender, ValueChangedEventArgs args)
    {
        if (args.Snapshot.Exists)
        {
            // Obtener el nombre del amigo emparejado cuando el campo matchedFriendName cambie
            string matchedFriendName = args.Snapshot.Value.ToString();
            friendNameText.text = matchedFriendName;
            Debug.Log($"Emparejamiento completado con {matchedFriendName}. ¡Listo para jugar!");
        }
    }

    private void OnDisable()
    {
        StopListeningForFriendStatusChanges();
        StopListeningForMatchedFriendNameChanges();
    }

    private void StopListeningForFriendStatusChanges()
    {
        friendsRef.ChildAdded -= HandleFriendStatusChange;
        friendsRef.ChildChanged -= HandleFriendStatusChange;
    }

    private void StopListeningForMatchedFriendNameChanges()
    {
        playersRef.Child(userId).Child("matchedFriendName").ValueChanged -= HandleMatchedFriendNameChange;
    }

    private void OnApplicationQuit()
    {
        Cancel();
    }

    public async void Cancel()
    {
        await LeaveMatchingQueue();
        AppNavegation.Instance.ShowProfileScreen();
    }

    public async Task LeaveMatchingQueue()
    {
        // Eliminar el estado de waiting y playing y poner el jugador como idle
        await playersRef.Child(userId).Child("status").SetValueAsync("idle");
        Debug.Log("Jugador ha salido de la cola de emparejamiento.");

        // Eliminar los datos del emparejamiento en ambos jugadores
        DataSnapshot friendIdSnapshot = await playersRef.Child(userId).Child("matchedFriendId").GetValueAsync();
        if (friendIdSnapshot.Exists)
        {
            string friendId = friendIdSnapshot.Value.ToString();
            // Eliminar los datos del amigo en el perfil del jugador
            await playersRef.Child(userId).Child("matchedFriendId").RemoveValueAsync();
            await playersRef.Child(userId).Child("matchedFriendName").RemoveValueAsync();

            // Eliminar los datos del jugador en el perfil del amigo
            await playersRef.Child(friendId).Child("matchedFriendId").RemoveValueAsync();
            await playersRef.Child(friendId).Child("matchedFriendName").RemoveValueAsync();

            Debug.Log("Datos de emparejamiento eliminados.");
        }

        // Volver a la pantalla de perfil u otra accion deseada
        AppNavegation.Instance.ShowProfileScreen();
    }
}
