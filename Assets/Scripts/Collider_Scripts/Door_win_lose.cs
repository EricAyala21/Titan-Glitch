using UnityEngine;

/*----------------------------------------------------------------------
- On trigger with player it will check the assigned value and pass it to
  the DoorSelected function in GameManager
  ----------------------------------------------------------------------
*/
public class Door_win_loss : MonoBehaviour
{
    [SerializeField] private DoorType doorType;
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.DoorSelected(doorType);
        }
    }
}