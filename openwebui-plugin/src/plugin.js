// OpenWebUI Plugin for Stability Matrix Chat Interface
class StabilityMatrixPlugin {
  async ready({ app, events }) {
    console.log("Stability Matrix Plugin loaded");

    // Register custom API endpoint
    events.on("customEndpoint", (endpoint) => {
      if (endpoint.path === "/api/v1/generate") {
        endpoint.method = "POST";
        endpoint.handler = async (req, res) => {
          const response = await fetch("http://stabilitymatrix-backend:5000/api/v1/generate", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(req.body),
          });
          return res.json(await response.json());
        };
      }
    });

    // Add UI elements to chat interface
    events.on("uiReady", () => {
      const generateButton = document.createElement("button");
      generateButton.textContent = "Generate";
      generateButton.className = "stability-matrix-generate-btn";
      
      const promptInput = document.querySelector(".prompt-input");
      if (promptInput) {
        promptInput.parentElement.appendChild(generateButton);
        
        generateButton.addEventListener("click", async () => {
          const prompt = promptInput.value;
          await handleGeneration(prompt);
        });
      }
    });
  }

  async handleGeneration(prompt) {
    // Show loading indicator
    const loadingIndicator = document.createElement("div");
    loadingIndicator.className = "loading-indicator";
    loadingIndicator.textContent = "Generating...";
    
    // Send request to backend
    try {
      const response = await fetch("/api/v1/generate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt }),
      });

      const result = await response.json();
      
      if (result.success) {
        // Display result
        displayResult(result);
      } else {
        showError(result.errorMessage);
      }
    } catch (error) {
      showError(error.message);
    }
  }
}

export default StabilityMatrixPlugin;
