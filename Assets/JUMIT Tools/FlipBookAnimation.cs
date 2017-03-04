//FLIP BOOK ANIMATION COMPONENT
using UnityEngine;
using System.Collections;
//------------------------------------------------
public class FlipBookAnimation : MonoBehaviour
{
    //Public variables
    public AtlasData AtlasData;
    //------------------------------------------------//Enum for Play Types
    public enum PlayType { Forward = 0, Reverse = 1, PingPong = 2, Custom = 3, Randomized = 4 };
    //Enum for Loop Type
    public enum LoopType { PlayOnce = 0, Loop = 1 };
    //Public reference to list of UVs for frames of flipbook animation
    public Rect[] UVs;
    //Reference to AutoPlay on Start
    public bool AutoPlay = false;
    //Public reference to number of frames per second for animation
    public float FramesPerSecond = 10.0f;
    //Public reference to play type of animation
    public PlayType PlayMethod = PlayType.Forward;
    //Public reference to loop type
    public LoopType LoopMethod = LoopType.PlayOnce;
    //Public reference to first frame. Custom setting used ONLY if PlayMethod==Custom. Otherwise, auto-calculated
    public int CustomStartFrame = 0;
    //Public reference to end frame. Custom setting used ONLY if PlayMethod==Custom. Otherwise, auto-calculated
    public int CustomEndFrame = 0;
    //Public reference to play status of flipbook animation
    public bool IsPlaying = false;

    
    //Methods
    //------------------------------------------------// Use this for initialization
    void Start()
    {
        //Initializing UVs regions
        initUVs();

        //Play animation if auto-play is true
        if (AutoPlay)
            StartCoroutine("Play");
    }

    //Init UVs from given Atlas
    private void initUVs()
    {
        //If an AtlasData was given:
        if (AtlasData != null)
            UVs = AtlasData.UVs;
        else
            print("No AtlasData for animation given.");
    }


    //------------------------------------------------//Function to play animation
    public IEnumerator Play()
    {
        //Set play status to true
        IsPlaying = true;
        //Get Anim Length in frames
        int AnimLength = UVs.Length;
        //Loop Direction
        int Direction = (PlayMethod == PlayType.Reverse) ? -1 : 1;
        //Start Frame for Forwards
        int StartFrame = (PlayMethod == PlayType.Reverse) ? AnimLength - 1 : 0;
        //Frame Count
        int FrameCount = AnimLength - 1;
        //if Animation length == 0 then exit
        if (FrameCount <= 0) yield break;
        //Check for custom frame overrides
        if (PlayMethod == PlayType.Custom)
        {
            StartFrame = CustomStartFrame;
            FrameCount = (CustomEndFrame > CustomStartFrame) ? CustomEndFrame - CustomStartFrame :
            CustomStartFrame - CustomEndFrame;
            Direction = (CustomEndFrame > CustomStartFrame) ? 1 : -1;
        }
        //Play back animation at least once
        do
        {
            //New playback cycle
            //Number of frames played
            int FramesPlayed = 0;
            //Play animation while all frames not played
            while (FramesPlayed <= FrameCount)
            {
                //Set frame - Get random frame if random, else get standard frame
                Rect Rct = (PlayMethod == PlayType.Randomized) ?
                UVs[Mathf.FloorToInt(Random.value * FrameCount)] : UVs[StartFrame + (FramesPlayed * Direction)];
                SetFrame(Rct);
                //Increment frame count
                FramesPlayed++;
                //Wait until next frame
                yield return new WaitForSeconds(1.0f / FramesPerSecond);
            }
            //If ping-pong, then reverse direction
            if (PlayMethod == PlayType.PingPong)
            {
                Direction = -Direction;
                StartFrame = (StartFrame == 0) ? AnimLength - 1 : 0;
            }
        } while (LoopMethod == LoopType.Loop); //Check for looping
        //Animation has ended. Set play status to false
        IsPlaying = false;
    }
    //------------------------------------------------//Function to stop playback
    public void Stop()
    {
        //If already stopped, then ignore
        if (!IsPlaying)
            return;
        StopCoroutine("Play");
        IsPlaying = false;
    }
    //------------------------------------------------//Function to set specified frame of mesh based on Rect UVs
    void SetFrame(Rect R)
    {
        //Get mesh filter
        Mesh MeshObject = GetComponent<MeshFilter>().mesh;
        //Vertices
        Vector3[] Vertices = MeshObject.vertices;
        Vector2[] UVs = new Vector2[Vertices.Length];
        //Bottom-left
        UVs[0].x = R.x;
        UVs[0].y = R.y;
        //Bottom-right
        UVs[1].x = R.x + R.width;
        UVs[1].y = R.y;
        //Top-left
        UVs[2].x = R.x;
        UVs[2].y = R.y + R.height;
        //Top-right
        UVs[3].x = R.x + R.width;
        UVs[3].y = R.y + R.height;
        MeshObject.uv = UVs;
        MeshObject.vertices = Vertices;
    }
    //------------------------------------------------//Function called on component disable
    void OnDisable()
    {
        //Stop coroutine if playing
        if (IsPlaying)
            StopCoroutine("Play");
    }
    //------------------------------------------------//Function called on component enable
    void OnEnable()
    {
        //If was playing before disabled, then start playing again
        if (IsPlaying)
            StartCoroutine("Play");
    }
    //------------------------------------------------
}