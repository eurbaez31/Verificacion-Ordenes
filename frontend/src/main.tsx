import { createRoot } from "react-dom/client";
import { MsalProvider } from "@azure/msal-react";
import { msalInstance, msalInitialization } from "./lib/msal";
import App from "./App.tsx";
import "./index.css";

msalInitialization.then(() => {
  createRoot(document.getElementById("root")!).render(
    msalInstance ? (
      <MsalProvider instance={msalInstance}>
        <App />
      </MsalProvider>
    ) : (
      <App />
    )
  );
});
