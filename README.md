# AR-Learn

AR-Learn is an AR-powered Unity app that enables immersive, agentic engineering education by fusing interactive 3D experiences with cutting-edge retrieval pipelines and backend AI systems.

***

### Core Features

- Interactive 3D Models: Prefabs and assets are tagged, enabling rich AR exploration and context-aware guidance for each part.
- AR Book-Scan Module: Scan textbook images or tokens using your device camera; recognized regions trigger 3D model spawning in the AR environment and context mapping.
- Part-Based Querying: Tap or select parts, triggering a retrieval-enhanced prompt sent to the backend, optimized for precise, part-associated answers.
- Audio Integration: Supports speech-to-text and text-to-speech flows, enabling natural guidance and conversational AR sessions.

***

### Hybrid RAG Workflow

AR-Learn leverages a two-leg retrieval-augmented generation (RAG) architecture for all queries from Unity:

1. **Ingest Phase (Backend)**  
   - Documents (PDFs or other resources) are parsed, chunked, and converted to embeddings, stored in ChromaDB (vector DB).  
   - Key entities, such as processes, functions, or part names, are mapped into Neo4j (graph DB).

2. **Query Phase (User Interaction)**  
   - When a user scans a book image or taps a part in Unity, the app sends context (tags, tokens, optionally audio input) to the backend.
   - The backend performs:
     - ChromaDB vector search (top-k document chunks).
     - Neo4j structured graph search for entity/relationship facts.
     - Reciprocal Rank Fusion to blend results for maximal relevance.
   - Result: Compressed, cited context returned from backend, used to guide user—visually, textually, and in audio.

***

### Book-Scan AR Module

- Uses image recognition to map scanned textbook regions (tokens) to corresponding 3D models.
- Spawns relevant prefabs into the AR environment, connecting real-world textbook cues with digital exploration.
- Integrates with part/tag system for fine-grained model queries.

***


### App-Screenshots
| Home | Jet-Engine | Refraction |
|--|--|--|
| ![Image1](https://github.com/user-attachments/assets/5281404e-736c-4ca5-924a-46372787637e) | ![Image2](https://github.com/user-attachments/assets/1e60acc6-a977-462f-93f0-61e5843dfbe6) | ![Image3](https://github.com/user-attachments/assets/7bc32102-c302-4bbb-925d-e8719f09c387) |

| UI | Architecture |
|--|--|
| ![](https://github.com/user-attachments/assets/0f9ad2ce-b93c-47ce-9a62-bd7a5cc24a3d) |![](https://github.com/user-attachments/assets/910b56d8-ea4e-4eab-817d-d1866c789034) |

---

### Getting Started

1. Clone the repo:  
   ```
   git clone https://github.com/Ashwani1330/AR-Learn.git
   ```
2. Open in Unity Hub (Unity 2021+ recommended).
3. Set up AR Foundation and required packages.
4. Import prefabs, models, and images matching your curriculum.
5. Ensure device camera permissions for book-scan module.
6. Configure backend endpoint connectivity for live RAG and part-query responses.

***

### Connecting to Backend

For full AI-powered retrieval, connect your Unity app to the FastAPI backend:

- Backend Repo: [AR-Learn-Backend](https://github.com/Ashwani1330/AR-Learn-Backend)
- All AR queries, scanned book tokens, and part taps should be routed to the backend API for smart, context-driven guidance.
- See backend’s README for setup and endpoint details.

***

### Folder Structure

```
├── Assets/
│   ├── Models/                  # 3D engineering assets
│   ├── Prefabs/                 # Interactive Unity prefabs
│   ├── Scripts/                 # AR, RAG, book-scan logic
│   ├── Scenes/                  # AR use-case scenes
│   └── ReferenceImageLibrary/   # Scannable textbook tokens/images
```

***

### Advanced Use

- Extend part-tag system and ReferenceImageLibrary to support additional textbooks or curricula.
- Implement custom guidance flows via backend RAG module—tune ranking, chunking, and prompt compression for best learning outcomes.
- Integrate additional AR interactions, bridging real-world cues and Unity digital context.

***

# References

- Main Backend: [AR-Learn-Backend](https://github.com/Ashwani1330/AR-Learn-Backend)
- For detailed backend setup and workflow, visit the backend repository.


