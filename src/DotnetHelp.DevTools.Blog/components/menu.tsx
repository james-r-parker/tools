import { buildBlogMenu } from "@/lib/blog";
import Link from "next/link";

function MenuItem({ text, link, selected }: { text: string, link: string, selected: boolean }) {
    return (
        <li>
            <Link className={selected ? "block border-l pl-4 -ml-px text-sky-500 border-current font-semibold dark:text-sky-400" : "block border-l pl-4 -ml-px border-transparent hover:border-slate-400 dark:hover:border-slate-500 text-slate-700 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-300"} href={link}>
                {text}
            </Link>
        </li>
    );
}

export default async function Menu() {

    const menu = await buildBlogMenu();

    return (
        <nav className="lg:text-sm lg:leading-6 relative">
            <ul>
                {Object.getOwnPropertyNames(menu).map(group => {
                    return (
                        <li key={group} className="mt-12 lg:mt-8">
                            <h5 className="mb-8 lg:mb-3 font-semibold text-slate-900 dark:text-slate-200">
                                {group}
                            </h5>
                            <ul className="space-y-6 lg:space-y-2 border-l border-slate-100 dark:border-slate-800">
                                {menu[group].map(item => {
                                    return <MenuItem key={item.link} text={item.text} link={item.link} selected={false} />
                                })}
                            </ul>
                        </li>
                    )
                })}
            </ul>
        </nav>
    );
}
