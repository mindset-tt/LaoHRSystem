"use client";

import { useState, useEffect } from "react";
import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip, ResponsiveContainer,
    PieChart, Pie, Cell, Legend
} from "recharts";
import { useTranslations } from "next-intl";
import { attendanceAPI, employeesAPI } from "@/lib/api";

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

export function AttendanceChart() {
    const [data, setData] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Generate last 7 days mock data for now as simplified real data aggregation 
        // requires complex backend logic not yet exposed in a simple endpoint
        // Ideally we would fetch: attendanceAPI.getWeeklySummary()

        // Simulating data for "Weekly Trends"
        const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
        const mockData = days.map(day => ({
            name: day,
            present: Math.floor(Math.random() * 20) + 100, // 100-120
            late: Math.floor(Math.random() * 10), // 0-10
            absent: Math.floor(Math.random() * 5) // 0-5
        }));

        setData(mockData);
        setLoading(false);
    }, []);

    if (loading) return <div className="h-64 flex items-center justify-center text-slate-500">Loading...</div>;

    return (
        <ResponsiveContainer width="100%" height="100%">
            <BarChart
                data={data}
                margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
            >
                <CartesianGrid strokeDasharray="3 3" stroke="#334155" vertical={false} />
                <XAxis dataKey="name" stroke="#94a3b8" fontSize={12} tickLine={false} axisLine={false} />
                <YAxis stroke="#94a3b8" fontSize={12} tickLine={false} axisLine={false} />
                <RechartsTooltip
                    contentStyle={{ backgroundColor: '#1e293b', borderColor: '#334155', color: '#f8fafc' }}
                    itemStyle={{ color: '#f8fafc' }}
                    cursor={{ fill: 'rgba(51, 65, 85, 0.4)' }}
                />
                <Bar dataKey="present" fill="#6366f1" radius={[4, 4, 0, 0]} stackId="a" />
                <Bar dataKey="late" fill="#f59e0b" radius={[4, 4, 0, 0]} stackId="a" />
                <Bar dataKey="absent" fill="#ef4444" radius={[4, 4, 0, 0]} stackId="a" />
            </BarChart>
        </ResponsiveContainer>
    );
}

export function DepartmentChart() {
    const [data, setData] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const employees = await employeesAPI.getAll();

                // Aggregate by department
                const deptCounts: Record<string, number> = {};
                employees.forEach(emp => {
                    const deptName = emp.department?.departmentNameEn || 'Unknown';
                    deptCounts[deptName] = (deptCounts[deptName] || 0) + 1;
                });

                const chartData = Object.entries(deptCounts).map(([name, value]) => ({
                    name,
                    value
                }));

                setData(chartData);
            } catch (error) {
                console.error("Failed to load department data", error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, []);

    if (loading) return <div className="h-64 flex items-center justify-center text-slate-500">Loading...</div>;
    if (data.length === 0) return <div className="h-64 flex items-center justify-center text-slate-500">No data available</div>;

    return (
        <ResponsiveContainer width="100%" height="100%">
            <PieChart>
                <Pie
                    data={data}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={80}
                    fill="#8884d8"
                    paddingAngle={5}
                    dataKey="value"
                >
                    {data.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                </Pie>
                <RechartsTooltip
                    contentStyle={{ backgroundColor: '#1e293b', borderColor: '#334155', color: '#f8fafc' }}
                    itemStyle={{ color: '#f8fafc' }}
                />
                <Legend
                    layout="vertical"
                    verticalAlign="middle"
                    align="right"
                    wrapperStyle={{ fontSize: '12px', color: '#94a3b8' }}
                />
            </PieChart>
        </ResponsiveContainer>
    );
}
