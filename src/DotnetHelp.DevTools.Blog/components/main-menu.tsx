import { buildPageMenu } from "@/lib/page";
import Link from "next/link";



function MenuItem({ text, link }: { text: string, link: string }) {
    return (
        <span className="block border-r pr-4 -mr-px border-transparent border-slate-400 dark:border-slate-500 text-slate-700 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-300">
            <Link className="text-sm" href={link}>
                {text}
            </Link>
        </span>
    );
}

export default async function MainMenu() {

    const menu = await buildPageMenu();

    return (
        <nav>
            <div className="flex gap-3">
                {menu.map(item => {
                    return <MenuItem key={item.link} text={item.text} link={item.link} />
                })}
                <span>
                    <Link className="text-sm" href={"https://www.dotnethelp.co.uk"}>
                        Developer Tools
                    </Link>
                </span>
            </div>
        </nav>
    );
}
