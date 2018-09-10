using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
    public HeightMapSettings[] heightMaps;
	public TextureData textureSettings;

	public Transform viewer;
	public Material mapMaterial;

	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	float meshWorldSize;
	int chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

	void Start() {

		textureSettings.ApplyToMaterial (mapMaterial);
		textureSettings.UpdateMeshHeights (mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

		float maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

		UpdateVisibleChunks ();
	}

	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);

		if (viewerPosition != viewerPositionOld) {
			foreach (TerrainChunk chunk in visibleTerrainChunks) {
				chunk.UpdateCollisionMesh ();
			}
		}

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks ();
		}
	}
		
	void UpdateVisibleChunks() {
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2> ();
		for (int i = visibleTerrainChunks.Count-1; i >= 0; i--) {
			alreadyUpdatedChunkCoords.Add (visibleTerrainChunks [i].coord);
			visibleTerrainChunks [i].UpdateTerrainChunk ();
		}
			
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / meshWorldSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / meshWorldSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunkCoords.Contains (viewedChunkCoord)) {
					if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
						terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
					} else {
                        //Get three side chunks cat get forwards chunk because it doesn't exist yet
                        //Gives chunk coordinate directly behind him
                        Vector2 behindChunkCoord = new Vector2(currentChunkCoordX + xOffset - 1, currentChunkCoordY + yOffset);
                        //Gives chunk coordinates for above and below
                        Vector2 aboveChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset + 1);
                        Vector2 belowChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset - 1);
                        TerrainChunk[] neighbours = new TerrainChunk[3];

             
                        //Try get the values and place in array
                        //Use try because we don't know if thye have all been loaded
                        terrainChunkDictionary.TryGetValue(behindChunkCoord,out neighbours[0]);
                        terrainChunkDictionary.TryGetValue(aboveChunkCoord, out neighbours[1]);
                        terrainChunkDictionary.TryGetValue(belowChunkCoord, out neighbours[2]);

                        int tot = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            if(neighbours[i] != null)
                               tot += neighbours[i].SettingIndex;                         
                        }

                        //Needs to be an int
                        int averageSettingIndex = tot / 3;

                        //Returns random value within specified range as to change the height setting but make it look normal
                        int chunkSetting = Random.Range(averageSettingIndex - 1, averageSettingIndex + 2);

                        if (chunkSetting < 0)
                            chunkSetting = 0;
                        else if (chunkSetting > heightMaps.Length)
                            chunkSetting = heightMaps.Length - 1;

                        print(chunkSetting);

                        TerrainChunk newChunk = new TerrainChunk (viewedChunkCoord,chunkSetting, heightMaps[chunkSetting],meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
						terrainChunkDictionary.Add (viewedChunkCoord, newChunk);
						newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
						newChunk.Load ();
                        print("new CHunk : " + newChunk.SettingIndex + "| Coord : " + newChunk.coord);
					}
				}

			}
		}
	}

	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
		if (isVisible) {
			visibleTerrainChunks.Add (chunk);
		} else {
			visibleTerrainChunks.Remove (chunk);
		}
	}

    void GetSettingOfChunk()
    {

    }
}

[System.Serializable]
public struct LODInfo {
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float visibleDstThreshold;


	public float sqrVisibleDstThreshold {
		get {
			return visibleDstThreshold * visibleDstThreshold;
		}
	}
}
