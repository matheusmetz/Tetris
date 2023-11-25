using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;
    
    private float stepTime;
    private float moveTime;
    private float lockTime;
    public bool canMove;
    public Board myboard;

    
    private Vector2 initialMousePosition;

    private void Start()
    {
        myboard = GetComponent<Board>();

        if (myboard == null)
        {
            Debug.LogError("Componente GameManager não encontrado. Certifique-se de que o objeto possui o componente GameManager.");
        }
        else
        {
            Debug.Log("Componente GameManager atribuído com sucesso.");
        }

        // Check if board is not initialized and initialize it
        if (board == null)
        {
            // You need to provide the appropriate values for position and data
            Initialize( board, position,  data);
        }
    }
    void Awake()
    {
        myboard = GetComponent<Board>();
        if (myboard == null)
        {
            Debug.LogError("GameManager não encontrado. Certifique-se de que a ordem de inicialização está correta.");
        }
    }


    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null) {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {

        if (myboard == null)
        {
            Debug.LogError("O componente GameManager não foi atribuído a 'myboard'. Certifique-se de que o objeto possui o componente GameManager.");
            return;
        }
        board.Clear(this);
        canMove = myboard.CanMove();
        // Mover a peça no "chão" antes dela travar
        lockTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
        myboard.PauseGame();

        }

         // Rotação:
    if (Input.GetKeyDown(KeyCode.Q) && canMove == true) 
    {
        Rotate(-1);
    }
    else if (Input.GetKeyDown(KeyCode.E) && canMove == true)
    {
        Rotate(1);
    }

    // Hard Drop
    if (Input.GetKeyDown(KeyCode.Space) && canMove == true)
    {
        HardDrop();
    }

    // Gestos de deslizamento para a esquerda e direita com o mouse
    if (Input.GetKeyDown(KeyCode.W) && canMove == true)
    {
        initialMousePosition = Input.mousePosition;
    }
    else if (Input.GetMouseButton(0) && canMove == true)
    {
        // Calcule a diferença entre as posições inicial e atual do mouse
        float mouseDeltaX = Input.mousePosition.x - initialMousePosition.x;

        // Mova a peça com base no gesto de deslizamento
        if (Mathf.Abs(mouseDeltaX) > 15f) // SENSIBILIDADE DO MOVIMENTO COM O MOUSE
        {
            if (mouseDeltaX > 0)
            {
                Move(Vector2Int.right);
            }
            else
            {
                Move(Vector2Int.left);
            }

            initialMousePosition = Input.mousePosition;
        }
    }

    // Allow the player to hold movement keys but only after a move delay
    // so it does not move too fast
    if (Time.time > moveTime)
    {
        HandleMoveInputs();
    }

    // "GRAVIDADE" da peça
    if (Time.time > stepTime)
    {
        Step();
    }

        board.Set(this);
    }

    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.S))
        {
            if (Move(Vector2Int.down)) {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.A)) {
            Move(Vector2Int.left);
        } else if (Input.GetKey(KeyCode.D)) {
            Move(Vector2Int.right);
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay) {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down)) {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation)) {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

}
