using UnityEngine;

public class PlayerPaddle : Paddle
{
    private Vector2 direction;

    public void SetPosition(float y)
    {
        transform.position = new Vector2(transform.position.x, y);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0)) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SetPosition(mousePosition.y);
        } 
        
    }

 

}
