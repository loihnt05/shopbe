"use client";

import React, { useState, useEffect, useRef } from "react";

export function Badge({ count }: { count: number }) {
  const [pop, setPop] = useState(false);
  const prev = useRef(count);

  useEffect(() => {
    if (count !== prev.current) {
      prev.current = count;
      // Use a timeout or requestAnimationFrame to defer state update
      const frame = requestAnimationFrame(() => setPop(true));
      const t = setTimeout(() => setPop(false), 300);
      return () => {
        cancelAnimationFrame(frame);
        clearTimeout(t);
      };
    }
  }, [count]);

  if (count === 0) return null;

  return (
    <span
      style={{
        position: "absolute",
        top: -8,
        right: -8,
        minWidth: 20,
        height: 20,
        padding: "0 5px",
        borderRadius: 12,
        background: "white",
        color: "red",
        fontSize: 11,
        fontWeight: 500,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        lineHeight: 1,
        fontFamily: "'DM Mono', monospace",
        letterSpacing: "-0.02em",
        transform: pop ? "scale(1.45)" : "scale(1)",
        transition: "transform 0.28s cubic-bezier(0.34,1.56,0.64,1)",
        pointerEvents: "none",
        zIndex: 10,
      }}
    >
      {count > 99 ? "99+" : count}
    </span>
  );
}
