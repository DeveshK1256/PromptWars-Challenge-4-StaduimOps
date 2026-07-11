import { Bot, Sparkles } from "lucide-react";
import { FormEvent, useCallback, useEffect, useState } from "react";
import type { AiAgent, AiChatResponse, AiKnowledgeDocument, ApiClient, ApiError } from "../services/api";
import type { AsyncStatus } from "../types";
import { SectionHeader } from "../components/SectionHeader";
import { StatusNotice } from "../components/StatusNotice";

export function AiAssistant({ client }: { client: ApiClient }) {
  const [status, setStatus] = useState<AsyncStatus>("idle");
  const [error, setError] = useState<ApiError | null>(null);
  const [messages, setMessages] = useState<Array<{ role: "user" | "assistant"; text: string; meta?: string }>>([]);
  const [agents, setAgents] = useState<AiAgent[]>([]);
  const [knowledge, setKnowledge] = useState<AiKnowledgeDocument[]>([]);

  const loadMetadata = useCallback(async () => {
    try {
      const [agentData, knowledgeData] = await Promise.all([client.aiAgents(), client.aiKnowledge()]);
      setAgents(agentData);
      setKnowledge(knowledgeData);
    } catch (caught) {
      setError(caught as ApiError);
    }
  }, [client]);

  useEffect(() => {
    void loadMetadata();
  }, [loadMetadata]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    const prompt = String(data.get("prompt") ?? "").trim();
    const context = String(data.get("context") ?? "").trim();
    if (!prompt) {
      setError({ title: "Validation failed", detail: "Prompt is required.", status: 400 });
      setStatus("error");
      return;
    }

    setMessages((existing) => [...existing, { role: "user", text: prompt }]);
    setStatus("loading");
    setError(null);
    try {
      const response: AiChatResponse = await client.aiChat({ prompt, context });
      setMessages((existing) => [
        ...existing,
        {
          role: "assistant",
          text: response.response,
          meta: `${response.agentName} - ${response.intent} - ${(response.confidenceScore * 100).toFixed(0)}% confidence - ${response.tokensUsed} tokens${response.escalationRecommended ? " - escalation advised" : ""}`,
        },
      ]);
      form.reset();
      setStatus("success");
    } catch (caught) {
      setStatus("error");
      setError(caught as ApiError);
    }
  }

  return (
    <section className="screen-grid">
      <SectionHeader
        icon={<Sparkles aria-hidden="true" />}
        title="AI assistant"
        description="Calls the Vertex AI Gemini adapter. Missing cloud configuration is reported as a service error."
      />
      <StatusNotice status={status} error={error} success="AI response received." />
      <article className="panel chat-panel">
        <div className="message-list" aria-live="polite">
          {messages.length === 0 && (
            <p className="muted">
              Ask for navigation, accessibility, transport, incident, or stadium operations guidance.
            </p>
          )}
          {messages.map((message, index) => (
            <div className={`message ${message.role}`} key={`${message.role}-${index}`}>
              <strong>{message.role === "user" ? "You" : "Assistant"}</strong>
              <p>{message.text}</p>
              {message.meta && <span>{message.meta}</span>}
            </div>
          ))}
        </div>
        <form className="form-grid" onSubmit={submit}>
          <label>
            Prompt
            <textarea
              name="prompt"
              rows={4}
              required
              placeholder="Find the least crowded accessible route to my seat."
            />
          </label>
          <label>
            Context
            <textarea name="context" rows={3} placeholder="Optional stadium, gate, seat, language, or role context." />
          </label>
          <button className="primary-action" type="submit" disabled={status === "loading"}>
            <Bot aria-hidden="true" />
            Send to AI service
          </button>
        </form>
      </article>
      <article className="panel split-panel">
        <div>
          <h2>Agent catalog</h2>
          <div className="list-stack">
            {agents.slice(0, 5).map((agent) => (
              <div className="row-card" key={agent.key}>
                <div>
                  <strong>{agent.displayName}</strong>
                  <span>{agent.intents.slice(0, 3).join(", ")}</span>
                </div>
                {agent.safetyCritical && <span className="status-pill warn">Safety</span>}
              </div>
            ))}
          </div>
        </div>
        <div>
          <h2>Knowledge sources</h2>
          <div className="list-stack">
            {knowledge.slice(0, 4).map((document) => (
              <div className="row-card" key={document.id}>
                <div>
                  <strong>{document.title}</strong>
                  <span>
                    {document.category} - {document.sourceType}
                  </span>
                </div>
                <span className="status-pill ok">{document.language}</span>
              </div>
            ))}
          </div>
        </div>
      </article>
    </section>
  );
}
