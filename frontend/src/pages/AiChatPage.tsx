import { useState, useRef, useEffect } from "react";
import { api } from "../hooks/useApi";

interface Message {
  id: string;
  role: "user" | "assistant";
  content: string;
  intent?: string;
  agentName?: string;
  confidence?: number;
  escalation?: boolean;
  timestamp: Date;
}

const SAMPLE_QUESTIONS = [
  "What's the fastest route to Section B from Gate 3?",
  "I need a wheelchair accessible route to the VIP area.",
  "What's the crowd situation at the south concourse?",
  "How do I get to the nearest metro from the stadium?",
  "Translate 'Your seat is in Section 114, Row 12' to Spanish.",
  "What are the sustainability initiatives at this stadium?",
];

export default function AiChatPage() {
  const [messages, setMessages] = useState<Message[]>([
    {
      id: "welcome",
      role: "assistant",
      content: "👋 Hi! I'm your FIFA World Cup 2026 Smart Stadium Assistant. I can help with navigation, crowd updates, transportation, accessibility, sustainability, and more. How can I help you today?",
      agentName: "Fan Assistant Agent",
      timestamp: new Date(),
    },
  ]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async (prompt: string) => {
    if (!prompt.trim() || loading) return;
    const userMsg: Message = {
      id: Date.now().toString(),
      role: "user",
      content: prompt.trim(),
      timestamp: new Date(),
    };
    setMessages(prev => [...prev, userMsg]);
    setInput("");
    setLoading(true);
    setError(null);
    try {
      const result = await api.aiChat(prompt.trim());
      const assistantMsg: Message = {
        id: (Date.now() + 1).toString(),
        role: "assistant",
        content: result.response ?? "I couldn't get a response. Please try again.",
        intent: result.intent,
        agentName: result.agentName,
        confidence: result.confidenceScore,
        escalation: result.escalationRecommended,
        timestamp: new Date(),
      };
      setMessages(prev => [...prev, assistantMsg]);
    } catch (e: any) {
      setError("The AI service is not available in demo mode. Configure Vertex AI credentials to enable live responses.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main id="main-content" className="flex flex-col h-[calc(100vh-4rem)] max-w-4xl mx-auto p-4">
      <div className="mb-4">
        <h1 className="text-2xl font-bold text-white">AI Stadium Assistant</h1>
        <p className="text-slate-400 text-sm mt-1">Powered by Vertex AI Gemini — 10 specialized agents</p>
      </div>

      {/* Suggestion chips */}
      <div className="flex flex-wrap gap-2 mb-4" aria-label="Sample questions">
        {SAMPLE_QUESTIONS.map(q => (
          <button
            key={q}
            onClick={() => sendMessage(q)}
            disabled={loading}
            className="text-xs bg-blue-900/40 hover:bg-blue-800/60 border border-blue-500/30 text-blue-300 rounded-full px-3 py-1.5 transition disabled:opacity-50"
          >
            {q}
          </button>
        ))}
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto space-y-4 mb-4 pr-1" role="log" aria-label="Chat messages" aria-live="polite">
        {messages.map(msg => (
          <div key={msg.id} className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}>
            <div className={`max-w-[80%] ${msg.role === "user" ? "order-2" : "order-1"}`}>
              {msg.role === "assistant" && (
                <div className="flex items-center gap-2 mb-1">
                  <span className="text-xs font-medium text-blue-400">{msg.agentName ?? "Assistant"}</span>
                  {msg.intent && <span className="text-xs text-slate-500">• {msg.intent}</span>}
                  {msg.escalation && <span className="text-xs bg-amber-500/20 text-amber-300 border border-amber-500/30 rounded px-1">Escalate to staff</span>}
                </div>
              )}
              <div className={`rounded-2xl px-4 py-3 ${
                msg.role === "user"
                  ? "bg-blue-600 text-white rounded-br-sm"
                  : "bg-white/10 border border-white/10 text-slate-200 rounded-bl-sm"
              }`}>
                <p className="text-sm whitespace-pre-wrap">{msg.content}</p>
              </div>
              <div className="text-xs text-slate-600 mt-1 px-1">
                {msg.timestamp.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
              </div>
            </div>
          </div>
        ))}
        {loading && (
          <div className="flex justify-start">
            <div className="bg-white/10 border border-white/10 rounded-2xl rounded-bl-sm px-4 py-3">
              <div className="flex gap-1" aria-label="Thinking...">
                <span className="w-2 h-2 bg-blue-400 rounded-full animate-bounce" style={{ animationDelay: "0ms" }} />
                <span className="w-2 h-2 bg-blue-400 rounded-full animate-bounce" style={{ animationDelay: "150ms" }} />
                <span className="w-2 h-2 bg-blue-400 rounded-full animate-bounce" style={{ animationDelay: "300ms" }} />
              </div>
            </div>
          </div>
        )}
        {error && <div role="alert" className="text-amber-300 text-sm bg-amber-900/20 border border-amber-500/30 rounded-xl p-3">{error}</div>}
        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <form onSubmit={e => { e.preventDefault(); sendMessage(input); }} className="flex gap-2" aria-label="Send a message">
        <label htmlFor="chat-input" className="sr-only">Type your message</label>
        <input
          id="chat-input"
          type="text"
          value={input}
          onChange={e => setInput(e.target.value.slice(0, 2000))}
          placeholder="Ask about navigation, crowd status, accessibility…"
          disabled={loading}
          className="flex-1 bg-white/10 border border-white/20 rounded-xl px-4 py-3 text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm disabled:opacity-50"
          aria-label="Message input"
          maxLength={2000}
        />
        <button
          type="submit"
          disabled={loading || !input.trim()}
          aria-label="Send message"
          className="bg-blue-600 hover:bg-blue-500 disabled:bg-slate-700 disabled:cursor-not-allowed text-white rounded-xl px-5 py-3 font-medium transition flex items-center gap-2"
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" /></svg>
          Send
        </button>
      </form>
      <div className="text-xs text-slate-600 text-right mt-1">{input.length}/2000</div>
    </main>
  );
}
