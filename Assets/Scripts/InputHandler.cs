using UnityEngine;

public class InputHandler : MonoBehaviour
{
    CoinTray highlightedTray;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (highlightedTray == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo) && hitInfo.transform.TryGetComponent(out CoinTray coinTray))
                {
                    highlightedTray = coinTray;
                    highlightedTray.Highlight(true);
                }
            }
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo) && hitInfo.transform.TryGetComponent(out CoinTray coinTray) && coinTray != highlightedTray && coinTray.CanRecieve(highlightedTray))
                {
                    coinTray.RecieveCoins(highlightedTray);
                    highlightedTray = null;
                }
                else
                {
                    highlightedTray.Highlight(false);
                    highlightedTray = null;
                }
            }
        }
    }
}
