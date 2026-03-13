// OpenWebUI Plugin for AuraFlow Studio Chat Interface
class AuraFlowPlugin {
  async ready({ app, events }) {
    console.log("AuraFlow Plugin loaded");

    // Register custom API endpoint
    events.on("customEndpoint", (endpoint) => {
      if (endpoint.path === "/api/v1/generate") {
        endpoint.method = "POST";
        endpoint.handler = async (req, res) => {
          const response = await fetch("http://auraflow-api:5000/api/v1/generation/generate", {
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
      generateButton.textContent = "Generate (AuraFlow)";
      generateButton.className = "auraflow-generate-btn";
      
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
    // Show loading indicator with progress bar
    const progressBar = document.createElement("div");
    progressBar.className = "auraflow-progress-bar";
    
    try {
      const response = await fetch("/api/v1/generate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt }),
      });

      const result = await response.json();
      
      if (result.success) {
        displayResult(result);
      } else {
        showError(result.errorMessage);
      }
    } catch (error) {
      showError(error.message);
    }
  }
}

export default AuraFlowPlugin;
